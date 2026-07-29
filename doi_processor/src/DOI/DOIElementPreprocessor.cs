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
        public static Dictionary<string, string> LoadDOIAliasListMapper(string dataFolderPath)
        {
            Dictionary<string, string> doiAliasListMapper = new Dictionary<string, string>();
            var crossRefDOIAliasListPath = CrossRefDOIToGZFileCache.GetAliasFilePath(dataFolderPath);
            var dummyDOIAliasListPath = DummyCacheManager.GetDOIAliasListFilePath(dataFolderPath);
            //var crossRefDOIAliasListFileInfo = new FileInfo(crossRefDOIAliasListPath);
            var crossRefDOIAliasMapper = CSVFunctions.ReadCSVAasDictionary(crossRefDOIAliasListPath);
            var dummyDOIAliasMapper = CSVFunctions.ReadCSVAasDictionary(dummyDOIAliasListPath);
            crossRefDOIAliasMapper.ToList().ForEach((v) =>
            {
                if (!dummyDOIAliasMapper.ContainsKey(v.Key))
                {
                    dummyDOIAliasMapper[v.Key] = v.Value;
                }
            });
            return dummyDOIAliasMapper;
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
        

        



        public static Dictionary<string, string> LoadISBNMapper(string dataFolderPath)
        {
            var isbnMapperOfCrossRef = CrossRefSubMapperBuilders.LoadISBNMapper(dataFolderPath);
            var isbnMapperOfDataCite = DataCiteSubMapperBuilders.LoadISBNMapper(dataFolderPath);
            var isbnMapper = new Dictionary<string, string>();
            isbnMapperOfCrossRef.ToList().ForEach((v) =>
            {
                isbnMapper[v.Key] = v.Value;
            });
            isbnMapperOfDataCite.ToList().ForEach((v) =>
            {
                isbnMapper[v.Key] = v.Value;
            });
            return isbnMapper;
        }
        public static Dictionary<string, List<string>> LoadTitleMapper(string dataFolderPath)
        {
            var titleMapperOfCrossRef = CrossRefSubMapperBuilders.LoadTitleMapper(dataFolderPath);
            var titleMapperOfDataCite = DataCiteSubMapperBuilders.LoadTitleMapper(dataFolderPath);
            var titleMapper = new Dictionary<string, List<string>>();
            titleMapperOfCrossRef.ToList().ForEach((v) =>
            {
                if (!titleMapper.ContainsKey(v.Key))
                {
                    titleMapper[v.Key] = new List<string>();
                }
                titleMapper[v.Key].AddRange(v.Value);
            });
            titleMapperOfDataCite.ToList().ForEach((v) =>
            {
                if (!titleMapper.ContainsKey(v.Key))
                {
                    titleMapper[v.Key] = new List<string>();
                }
                titleMapper[v.Key].AddRange(v.Value);
            });
            return titleMapper;
        }
        public static Dictionary<string, string> LoadISSNMapper(string dataFolderPath)
        {
            var issnMapperOfCrossRef = CrossRefSubMapperBuilders.LoadISSNMapper(dataFolderPath);
            var issnMapperOfDataCite = DataCiteSubMapperBuilders.LoadISSNMapper(dataFolderPath);
            var issnMapper = new Dictionary<string, string>();
            issnMapperOfCrossRef.ToList().ForEach((v) =>
            {
                issnMapper[v.Key] = v.Value;
            });
            issnMapperOfDataCite.ToList().ForEach((v) =>
            {
                issnMapper[v.Key] = v.Value;
            });
            return issnMapper;
        }


       

        



        private static async Task MainLoop(string dataFolderPath, string mailAddress, SmallCacheManager smallCacheManager, HashSet<string> crossRefDOIPrefixSet, HashSet<string> dataCiteDOIPrefixSet)
        {
            var round = 0;

            /*
            var crossRefSmallCache = new CrossRefSmallCache();
            crossRefSmallCache.Load(dataFolderPath);
            var dataCiteSmallCache = new DataCiteSmallCache();
            dataCiteSmallCache.Load(dataFolderPath);

            var dummyDOIElementDict = DOIElement.Load(DummyCacheManager.GetDummyCacheFilePath(dataFolderPath), false);
            */

            var dblpSeriesDictionary = DBLPProceedingsSeriesDictionary.Load(dataFolderPath + "/auto_generated/cache/dblp_cache/dblp_proceedings.jsonl");
            dblpSeriesDictionary.BuildDoiToSeriesTitleAndKeyMapper();



            CommonFunctions.OutputSystemMessageFunction("Building SmallCache [START]");
            CommonFunctions.IncrementParagraphCounter();
            while (true)
            {
                round++;
                CommonFunctions.OutputSystemMessageFunction("Round: " + round, ConsoleColor.Green);
                foreach (var v in smallCacheManager.DOICacheInfoDict.Values)
                {
                    if (v.SourceCite.Length == 0)
                    {
                        v.UpdateSourceCite(crossRefDOIPrefixSet, dataCiteDOIPrefixSet);
                    }
                }


                await DataProcessor.CrossRefCacheBuilder.UpdateSmallCache(dataFolderPath, smallCacheManager.DOICacheInfoDict, smallCacheManager.CrossRefSmallCache, mailAddress);
                await DataProcessor.DataCitePreprocessor.UpdateSmallCache(dataFolderPath, smallCacheManager.DOICacheInfoDict, smallCacheManager.DataCiteSmallCache, mailAddress);
                smallCacheManager.MergeCheck();                
                smallCacheManager.UpdateContainerDOI(dataFolderPath);
                smallCacheManager.UpdateModifiedTitleUsingDBLP(dataFolderPath, dblpSeriesDictionary);
                smallCacheManager.InsertDOICacheInfoUsingSecondaryDOI(dataFolderPath);
                smallCacheManager.ModifyType(dataFolderPath);
                smallCacheManager.CacheConnectionCheck();
                //smallCacheManager.InsertDOICacheInfoUsingContainerDOI(dataFolderPath);
                //UpdateDummyDOI(dataFolderPath, smallCacheManager.DOICacheInfoDict);


                int unknownCounter = smallCacheManager.DOICacheInfoDict.Values.Count(v => v.SourceStatus == "");
                if (unknownCounter == 0) { break; }
                Console.WriteLine("Waiting for update... " + unknownCounter + " unknown DOIs");

            }


            {
                var doiElementDict = smallCacheManager.CreateDOIElementDictionaryFromSmallCache(dataFolderPath);
                doiElementDict.Values.ToList().ForEach((v) =>
                {
                    if (smallCacheManager.DOICacheInfoDict.ContainsKey(v.DOI))
                    {
                        var w = smallCacheManager.DOICacheInfoDict[v.DOI];
                        w.ISList.Clear();
                        v.ISBNList.ForEach((isbn) =>
                        {
                            w.ISList.Add("ISBN:" + isbn);
                        });
                        v.ISSNList.ForEach((issn) =>
                        {
                            w.ISList.Add("ISSN:" + issn);
                        });
                    }

                });
            }


            CommonFunctions.DecrementParagraphCounter();
            CommonFunctions.OutputSystemMessageFunction("Building SmallCache [END]");





            smallCacheManager.CrossRefSmallCache.Save(dataFolderPath);
            smallCacheManager.DataCiteSmallCache.Save(dataFolderPath);
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

            var smallCacheManager = new SmallCacheManager(dataFolderPath, primaryDOISet);
            var crossRefDOIPrefixSet = CrossRefDOIToGZFileCache.GetDOIPrefixSet(dataFolderPath);
            var dataCiteDOIPrefixSet = DataCiteDOIToGZFileCache.GetDOIPrefixSet(dataFolderPath);

            await MainLoop(dataFolderPath, mailAddress, smallCacheManager, crossRefDOIPrefixSet, dataCiteDOIPrefixSet);


            DOIElement.Save(smallCacheManager.DummyDOIElementDict, DummyCacheManager.GetDummyCacheFilePath(dataFolderPath));
            DOICacheInfo.Save(smallCacheManager.DOICacheInfoDict, doiCacheInfoFilePath);
            WriteChecksum(dataFolderPath, checksumFileName, primaryDOISet);

            Console.WriteLine("Building SmallCache [END]");
        }


    }
}