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
            return dataFolderPath + "/auto_generated/cache/small_cache_summary.jsonl";
        }

        public static string GetDummyDOIElementCachePath(string dataFolderPath)
        {
            return dataFolderPath + "/auto_generated/cache/dummy_cache/doi_element.jsonl";
        }
        public static string GetDebugLogFolderPath(string dataFolderPath)
        {
            return dataFolderPath + "/auto_generated/cache/debug_log";
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
        public static Dictionary<string, DOIElement> CreateDOIElementDictionaryFromSmallCache(string dataFolderPath, IDictionary<string, DOICacheInfo> doiCacheInfoDict)
        {
            var crossRefDOIPrefixSet = CrossRefDOIToGZFileCache.GetDOIPrefixSet(dataFolderPath);
            var crossRefdoiElementDict = CrossRefCacheBuilder.LoadSmallCache(dataFolderPath, doiCacheInfoDict, crossRefDOIPrefixSet);

            var dataCiteDOIPrefixSet = DataCiteDOIToGZFileCache.GetDOIPrefixSet(dataFolderPath);
            var dataCitedoiElementDict = DataCitePreprocessor.LoadSmallCache(dataFolderPath, doiCacheInfoDict, dataCiteDOIPrefixSet);

            var mergedDict = new Dictionary<string, DOIElement>();
            crossRefdoiElementDict.ToList().ForEach((v) =>
            {
                mergedDict[v.Key] = v.Value;
            });
            dataCitedoiElementDict.ToList().ForEach((v) =>
            {
                mergedDict[v.Key] = v.Value;
            });

            doiCacheInfoDict.Values.ToList().ForEach((v) =>
            {
                if (!mergedDict.ContainsKey(v.DOI))
                {
                    var doiElement = new DOIElement() { DOI = v.DOI, Source = "Unknown", IsPrimary = v.DOIRank == 0 };
                    mergedDict[v.DOI] = doiElement;
                }
            });



            return mergedDict;

        }

        public static void UpdateSecondaryDOI(string dataFolderPath, IDictionary<string, DOICacheInfo> doiCacheInfoDict)
        {
            var doiElementDict = CreateDOIElementDictionaryFromSmallCache(dataFolderPath, doiCacheInfoDict);

            doiElementDict.Values.ToList().ForEach((v) =>
            {
                if (doiCacheInfoDict.ContainsKey(v.DOI))
                {
                    var w = doiCacheInfoDict[v.DOI];
                    if (w.ContainerDOI.Length > 0 && !doiCacheInfoDict.ContainsKey(w.ContainerDOI))
                    {
                        doiCacheInfoDict[w.ContainerDOI] = new DOICacheInfo() { DOI = w.ContainerDOI, DOIRank = 1 };
                    }
                }

                if (v.IsPrimary)
                {
                    v.DOIReferences.ForEach((referenceDOI) =>
                    {
                        if (!doiCacheInfoDict.ContainsKey(referenceDOI))
                        {
                            doiCacheInfoDict[referenceDOI] = new DOICacheInfo() { DOI = referenceDOI, DOIRank = 1 };
                        }
                    });

                }

            });
        }

        public static void UpdateDummyDOI(string dataFolderPath, IDictionary<string, DOICacheInfo> doiCacheInfoDict)
        {
            var dummyDirectoryPath = dataFolderPath + "/auto_generated/cache/dummy_cache";
            if (!Directory.Exists(dummyDirectoryPath))
            {
                Directory.CreateDirectory(dummyDirectoryPath);
            }


            Dictionary<string, DOIElement> dummyDOIElementDict = DOIElement.Load(GetDummyDOIElementCachePath(dataFolderPath), false);

            var doiElementDict = CreateDOIElementDictionaryFromSmallCache(dataFolderPath, doiCacheInfoDict);

            doiCacheInfoDict.Values.ToList().ForEach((v) =>
            {
                if (doiElementDict.ContainsKey(v.DOI))
                {
                    var vInfo = doiElementDict[v.DOI];
                    if (v.ContainerDOI != null && !doiCacheInfoDict.ContainsKey(v.ContainerDOI))
                    {
                        var containerTitle = vInfo.ContainerTitle;
                        var containerDOI = vInfo.ContainerDOI;

                        var newDOIElement = new DOIElement() { DOI = containerDOI, Title = containerTitle, Source = "Unknown", IsPrimary = false, Type = "Book?" };
                        dummyDOIElementDict[newDOIElement.DOI] = newDOIElement;
                        doiCacheInfoDict[newDOIElement.DOI] = new DOICacheInfo() { DOI = newDOIElement.DOI, DOIRank = 1, SourceCite = "Dummy", SourceStatus = "Unknown" };
                    }
                    else if (v.ContainerDOI == null)
                    {
                        if (vInfo.ISBNList.Count > 0)
                        {
                            var isbn = vInfo.ISBNList[0];
                            var containerDOI = "dummy/isbn/" + isbn;
                            var newDOIElement = new DOIElement() { DOI = containerDOI, Title = "???", Source = "Unknown", IsPrimary = false, Type = "Book?" };
                            dummyDOIElementDict[newDOIElement.DOI] = newDOIElement;
                            doiCacheInfoDict[newDOIElement.DOI] = new DOICacheInfo() { DOI = newDOIElement.DOI, DOIRank = 1, SourceCite = "Dummy", SourceStatus = "Unknown" };
                        }
                    }

                }
            });
            DOIElement.Save(dummyDOIElementDict, GetDummyDOIElementCachePath(dataFolderPath));

            var debugLogFolderPath = GetDebugLogFolderPath(dataFolderPath);
            if (!Directory.Exists(debugLogFolderPath))
            {
                Directory.CreateDirectory(debugLogFolderPath);
            }


            Dictionary<string, string> isbnToDOIMapper = new Dictionary<string, string>();
            doiElementDict.Values.ToList().ForEach((v) =>
            {
                bool parent_check = ISBNConverter.isISBNOwner(v.Type);

                if (parent_check)
                {
                    v.ISBNList.ForEach((isbn) =>
                    {
                        isbnToDOIMapper[isbn] = v.DOI;
                    });
                }
            });

            doiElementDict.Values.ToList().ForEach((v) =>
            {
                bool parent_check = ISBNConverter.isISBNOwner(v.Type);

                if (!parent_check)
                {
                    v.ISBNList.ForEach((isbn) =>
                    {
                        if (!isbnToDOIMapper.ContainsKey(isbn))
                        {
                            isbnToDOIMapper[isbn] = "null";
                        }
                    });
                }
            });

            CSVFunctions.WriteCSVAsDictionary(debugLogFolderPath + "/isbnToDOIMapper.jsonl", isbnToDOIMapper);





        }



        public static async Task BuildSmallCaches(string dataFolderPath, string mailAddress, ReadOnlySet<string> primaryDOISet, string checksumFileName)
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

            if (new FileInfo(doiCacheInfoFilePath).Exists)
            {
                doiCacheInfoDict = DOICacheInfo.Load(doiCacheInfoFilePath);
            }


            doiCacheInfoDict.Values.ToList().ForEach((v) =>
            {
                v.DOIRank = 1;
            });

            primaryDOISet.ToList().ForEach((v) =>
            {
                if (!doiCacheInfoDict.ContainsKey(v))
                {
                    doiCacheInfoDict[v] = new DOICacheInfo() { DOI = v, DOIRank = 0 };
                }
                else
                {
                    doiCacheInfoDict[v].DOIRank = 0;
                }
            });

            var crossRefDOIPrefixSet = CrossRefDOIToGZFileCache.GetDOIPrefixSet(dataFolderPath);
            var dataCiteDOIPrefixSet = DataCiteDOIToGZFileCache.GetDOIPrefixSet(dataFolderPath);




            while (true)
            {
                foreach (var v in doiCacheInfoDict.Values)
                {
                    if (v.SourceCite.Length == 0)
                    {
                        v.UpdateSourceCite(crossRefDOIPrefixSet, dataCiteDOIPrefixSet);
                    }
                }


                await DataProcessor.CrossRefCacheBuilder.UpdateSmallCache(dataFolderPath, doiCacheInfoDict, mailAddress);
                await DataProcessor.DataCitePreprocessor.UpdateSmallCache(dataFolderPath, doiCacheInfoDict, mailAddress);
                UpdateContainerDOI(dataFolderPath, doiCacheInfoDict, crossRefDOIPrefixSet);
                UpdateSecondaryDOI(dataFolderPath, doiCacheInfoDict);
                UpdateDummyDOI(dataFolderPath, doiCacheInfoDict);

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

        /*
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
                */



        public static void UpdateContainerDOI(string dataFolderPath, IDictionary<string, DOICacheInfo> doiCacheInfoDict, HashSet<string> crossRefDOIPrefixSet)
        {
            var logFilePath = dataFolderPath + "/auto_generated/log/update_container_doi.log";
            var logFile = new StreamWriter(logFilePath, true);
            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : Start");


            //var filePath = CrossRefDOIToGZFileCache.GetDOIToGZFileFolderPath(dataFolderPath);

            var isbnFilePath = CrossRefDOIToGZFileCache.GetISBNFilePath(dataFolderPath);
            var isbnDictionary = CSVFunctions.ReadCSVAasDictionary(isbnFilePath);
            var titleFilePath = CrossRefDOIToGZFileCache.GetTitleFilePath(dataFolderPath);
            var titleDictionary = CSVFunctions.ReadCSVAasDictionary(titleFilePath);
            var issnFilePath = CrossRefDOIToGZFileCache.GetISSNFilePath(dataFolderPath);
            var issnDictionary = CSVFunctions.ReadCSVAasDictionary(issnFilePath);

            var doiElementDict = CreateDOIElementDictionaryFromSmallCache(dataFolderPath, doiCacheInfoDict);



            doiCacheInfoDict.Values.ToList().ForEach((w) =>
            {
                w.UpdateContainerDOI(doiElementDict[w.DOI], isbnDictionary, titleDictionary, logFile);
            });

            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : End");
            logFile.Close();

        }



    }
}