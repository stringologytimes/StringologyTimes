using System;
using System.IO;
using System.Text;
using System.Linq;
using CommandLine;
using System.Threading.Tasks;
using DataProcessor;
using System.Text.Json;
using System.Collections.ObjectModel;

namespace DataProcessor
{
    class DOIElementPreprocessor
    {


        public static string GetCachePath(string dataFolderPath)
        {
            return dataFolderPath + "/auto_generated/cache/doi_element.jsonl";
        }

        public static string GetDOICacheInfoPath(string dataFolderPath)
        {
            return dataFolderPath + "/auto_generated/cache/doi_cache_info.jsonl";
        }
        public static Dictionary<string, string> LoadDOIPrefixMapper(string dataFolderPath)
        {
            Dictionary<string, string> doiPrefixMapper = new Dictionary<string, string>();
            var crossRefDOIPrefixPath = CrossRefDOIToGZFileCache.GetDOIPrefixFilePath(dataFolderPath);
            var crossRefDOIPrefixFileInfo = new FileInfo(crossRefDOIPrefixPath);
            if (crossRefDOIPrefixFileInfo.Exists)
            {
                var lines = File.ReadAllLines(crossRefDOIPrefixPath);
                foreach (var doiPrefix in lines)
                {
                    doiPrefixMapper[doiPrefix] = "CrossRef";
                }
            }

            var dataCiteDOIPrefixPath = DataCiteDOIToGZFileCache.GetDOIPrefixFilePath(dataFolderPath);
            var dataCiteDOIPrefixFileInfo = new FileInfo(dataCiteDOIPrefixPath);
            if (dataCiteDOIPrefixFileInfo.Exists)
            {
                var lines = File.ReadAllLines(dataCiteDOIPrefixPath);
                foreach (var doiPrefix in lines)
                {
                    doiPrefixMapper[doiPrefix] = "DataCite";
                }
            }

            return doiPrefixMapper;
        }

        /*
        public static Dictionary<string, DOIElement> LoadSmallCache(string dataFolderPath)
        {
            var doiElementDict = new Dictionary<string, DOIElement>();

            var crossRefDic = DataProcessor.CrossRefPreprocessor.LoadSmallCache(dataFolderPath);
            var dataCiteDic = DataProcessor.DataCitePreprocessor.LoadSmallCache(dataFolderPath);

            crossRefDic.ToList().ForEach((v) =>
            {
                doiElementDict[v.Key] = v.Value;
            });
            dataCiteDic.ToList().ForEach((v) =>
            {
                doiElementDict[v.Key] = v.Value;
            });
            return doiElementDict;
        }
        */

        public static void LaunchSmallCacheBuilder()
        {
            throw new Exception("Not implemented");
        }

        private static void WriteChecksum(string dataFolderPath, string checksumFileName, ReadOnlySet<string> doiSet)
        {
            var currentChecksumDictionary = new Dictionary<string, string>();
            currentChecksumDictionary["doiSet_hash"] = HashFunctions.ComputeHash(doiSet);
            currentChecksumDictionary["date"] = DateTime.Now.ToString("yyyy-MM");
            var checksumFilePath = dataFolderPath + "/auto_generated/cache/" + checksumFileName;
            CSVFunctions.WriteCSVAsDictionary(checksumFilePath, currentChecksumDictionary);
        }

        private static bool ChecksumCheck(string dataFolderPath, string checksumFileName, ReadOnlySet<string> doiSet)
        {
            var checksumFilePath = dataFolderPath + "/auto_generated/cache/" + checksumFileName;

            var currentChecksumDictionary = new Dictionary<string, string>();
            currentChecksumDictionary["doiSet_hash"] = HashFunctions.ComputeHash(doiSet);
            currentChecksumDictionary["date"] = DateTime.Now.ToString("yyyy-MM");


            if (new FileInfo(checksumFilePath).Exists)
            {
                var checksumDictionary_tmp = CSVFunctions.ReadCSVAasDictionary(checksumFilePath);
                var b = true;

                foreach (var item in currentChecksumDictionary)
                {
                    if (checksumDictionary_tmp.ContainsKey(item.Key))
                    {
                        if (checksumDictionary_tmp[item.Key] != item.Value)
                        {
                            b = false;
                        }
                    }
                    else
                    {
                        b = false;
                    }
                }

                if (b)
                {
                    return true;
                }
            }
            return false;
        }

        public static void UpdateSecondaryDOI(string dataFolderPath, IDictionary<string, DOICacheInfo> doiCacheInfoDict)
        {
            var crossRefDOIPrefixSet = CrossRefDOIToGZFileCache.GetDOIPrefixSet(dataFolderPath);
            var crossRefdoiElementDict = CrossRefCacheBuilder.LoadSmallCache(dataFolderPath, doiCacheInfoDict, crossRefDOIPrefixSet);

            var dataCiteDOIPrefixSet = DataCiteDOIToGZFileCache.GetDOIPrefixSet(dataFolderPath);
            var dataCitedoiElementDict = DataCitePreprocessor.LoadSmallCache(dataFolderPath, doiCacheInfoDict, dataCiteDOIPrefixSet);

            var mergedList = new List<DOIElement>();
            mergedList.AddRange(crossRefdoiElementDict.Values);
            mergedList.AddRange(dataCitedoiElementDict.Values);

            mergedList.ForEach((v) =>
            {
                if (doiCacheInfoDict.ContainsKey(v.DOI))
                {
                    var w = doiCacheInfoDict[v.DOI];
                    if (w.ContainerDOI.Length > 0 && !doiCacheInfoDict.ContainsKey(w.ContainerDOI))
                    {
                        doiCacheInfoDict[w.ContainerDOI] = new DOICacheInfo() { DOI = w.ContainerDOI, Priority = 1 };
                    }
                }

                if (v.IsPrimary)
                {
                    v.DOIReferences.ForEach((referenceDOI) =>
                    {
                        if (!doiCacheInfoDict.ContainsKey(referenceDOI))
                        {
                            doiCacheInfoDict[referenceDOI] = new DOICacheInfo() { DOI = referenceDOI, Priority = 1 };
                        }
                    });

                }

            });
        }


        public static async Task BuildSmallCacheX(string dataFolderPath, string mailAddress, ReadOnlySet<string> primaryDOISet, string checksumFileName)
        {
            var logFolderPath = dataFolderPath + "/auto_generated/log";

            var checksumFilePath = dataFolderPath + "/auto_generated/cache/" + checksumFileName;
            var doiCacheInfoFilePath = GetDOICacheInfoPath(dataFolderPath);

            Console.WriteLine("Building SmallCache [START]");

            bool b = ChecksumCheck(dataFolderPath, checksumFileName, primaryDOISet);
            if (b)
            {
                Console.WriteLine("SmallCache already exists [END]");
                return;
            }

            var doiCacheInfoDict = new Dictionary<string, DOICacheInfo>();

            if(new FileInfo(doiCacheInfoFilePath).Exists)
            {
                doiCacheInfoDict = DOICacheInfo.Load(doiCacheInfoFilePath);
            }


            doiCacheInfoDict.Values.ToList().ForEach((v) =>
            {
                v.Priority = 1;
            });

            primaryDOISet.ToList().ForEach((v) =>
            {
                if(!doiCacheInfoDict.ContainsKey(v))
                {
                    doiCacheInfoDict[v] = new DOICacheInfo() { DOI = v, Priority = 0 };
                }
                else
                {
                    doiCacheInfoDict[v].Priority = 0;
                }
            });

            var crossRefDOIPrefixSet = CrossRefDOIToGZFileCache.GetDOIPrefixSet(dataFolderPath);
            var dataCiteDOIPrefixSet = DataCiteDOIToGZFileCache.GetDOIPrefixSet(dataFolderPath);


            while (true)
            {
                foreach (var v in doiCacheInfoDict.Values)
                {
                    var doiPrefix = DOIFunctions.GetPrefix(v.DOI);
                    if (crossRefDOIPrefixSet.Contains(doiPrefix))
                    {
                        v.SourceCite = "CrossRef";
                    }
                    else if (dataCiteDOIPrefixSet.Contains(doiPrefix))
                    {
                        v.SourceCite = "DataCite";
                    }
                    else
                    {
                        v.SourceCite = "Unknown";
                        v.SourceStatus = "Unknown";
                    }
                }


                await DataProcessor.CrossRefCacheBuilder.UpdateSmallCache(dataFolderPath, doiCacheInfoDict, mailAddress);
                await DataProcessor.DataCitePreprocessor.UpdateSmallCache(dataFolderPath, doiCacheInfoDict, mailAddress);
                UpdateSecondaryDOI(dataFolderPath, doiCacheInfoDict);

                int unknownCounter = doiCacheInfoDict.Values.Count(v => v.SourceStatus == "");
                if (unknownCounter == 0) { break; }
                Console.WriteLine("Waiting for update... " + unknownCounter + " unknown DOIs");

            }



            DOICacheInfo.Save(doiCacheInfoDict, doiCacheInfoFilePath);
            WriteChecksum(dataFolderPath, checksumFileName, primaryDOISet);



            //UpdateSecondaryDOI(dataFolderPath, doiCacheInfoDict);










            //DataProcessor.CrossRefCacheBuilder.UpdateSmallCacheUsingContainerDOI(dataFolderPath);
            //DataProcessor.DataCitePreprocessor.UpdateSmallCacheUsingContainerTitle(dataFolderPath);

            //DataProcessor.CrossRefCacheBuilder.UpdateSmallCacheUsingISBN(dataFolderPath);


            //DataProcessor.CrossRefCacheBuilder.UpdateSmallCacheUsingDOIPrefix(dataFolderPath);
            //DataProcessor.DataCitePreprocessor.UpdateSmallCacheUsingDOIPrefix(dataFolderPath);



            /*
                        var doiElementDict = new Dictionary<string, DOIElement>();

                        var crossRefDic = DataProcessor.CrossRefCacheBuilder.LoadSmallCache(dataFolderPath);
                        var dataCiteDic = DataProcessor.DataCitePreprocessor.LoadSmallCache(dataFolderPath);

                        crossRefDic.ToList().ForEach((v) =>
                        {
                            doiElementDict[v.Key] = v.Value;
                        });
                        dataCiteDic.ToList().ForEach((v) =>
                        {
                            doiElementDict[v.Key] = v.Value;
                        });
                        */




            //await SemanticScholarPreprocessor.PreprocessAll(doiElementDict, dataFolderPath);



            //DOIElement.Save(doiElementDict, GetCachePath(dataFolderPath));


            Console.WriteLine("Building SmallCache [END]");
        }

        public static Dictionary<string, DOIElement> BuildDOIElementDictionary(string dataFolderPath, ReadOnlySet<string> doiSet)
        {
            var logFilePath = dataFolderPath + "/auto_generated/log/build_doi_element_dictionary.log";
            var logFile = new StreamWriter(logFilePath, true);
            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : Start");


            var doiElementDict = new Dictionary<string, DOIElement>();
            var doiElementCachePath = DOIElementPreprocessor.GetCachePath(dataFolderPath);
            var doiElementCache = DOIElement.Load(doiElementCachePath, false);
            doiSet.ToList().ForEach((v) =>
            {
                if (doiElementCache.ContainsKey(v))
                {
                    doiElementDict[v] = doiElementCache[v];
                }
                else
                {
                    logFile.WriteLine("DOI not found in cache: " + v);
                }
            });

            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : End");
            logFile.Close();
            return doiElementDict;
        }



    }
}