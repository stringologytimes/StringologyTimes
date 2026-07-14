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

        private static void InsertDOICacheInfoUsingSecondaryDOI(string dataFolderPath, IDictionary<string, DOICacheInfo> doiCacheInfoDict)
        {
            var doiElementDict = CreateDOIElementDictionaryFromSmallCache(dataFolderPath, doiCacheInfoDict);

            doiElementDict.Values.ToList().ForEach((v) =>
            {
                if (doiCacheInfoDict.ContainsKey(v.DOI))
                {
                    var w = doiCacheInfoDict[v.DOI];
                    if (w.ProperContainerDOI.Length > 0 && !doiCacheInfoDict.ContainsKey(w.ProperContainerDOI))
                    {
                        doiCacheInfoDict[w.ProperContainerDOI] = new DOICacheInfo() { DOI = w.ProperContainerDOI, DOIRank = 1 };
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
        private static void InsertDOICacheInfoUsingContainerDOI(string dataFolderPath, IDictionary<string, DOICacheInfo> doiCacheInfoDict)
        {
            var doiElementDict = CreateDOIElementDictionaryFromSmallCache(dataFolderPath, doiCacheInfoDict);
            var isbnDictionary = LoadISBNMapper(dataFolderPath);
            doiElementDict.Values.ToList().ForEach((v) =>
            {
                if (doiCacheInfoDict.ContainsKey(v.DOI))
                {
                    var w = doiCacheInfoDict[v.DOI];
                    if (w.ProperContainerDOI.Length > 0 && !doiCacheInfoDict.ContainsKey(w.ProperContainerDOI))
                    {
                        doiCacheInfoDict[w.ProperContainerDOI] = new DOICacheInfo() { DOI = w.ProperContainerDOI, DOIRank = 1 };
                    }
                    /*
                    v.ISBNList.ForEach((isbn) =>
                    {
                        if (isbnDictionary.ContainsKey(isbn))
                        {
                            var containerDOI = isbnDictionary[isbn];
                            if (v.DOI != containerDOI && !doiCacheInfoDict.ContainsKey(containerDOI))
                            {
                                doiCacheInfoDict[containerDOI] = new DOICacheInfo() { DOI = containerDOI, DOIRank = 1 };

                                var cache = doiCacheInfoDict[v.DOI];
                                cache.ProperContainerDOI = containerDOI;

                            }

                        }

                    });
                    */

                }
            });
        }

        /*
        public static void InsertDOICacheInfoUsingTitle(string dataFolderPath, IDictionary<string, DOICacheInfo> doiCacheInfoDict)
        {
            var doiElementDict = CreateDOIElementDictionaryFromSmallCache(dataFolderPath, doiCacheInfoDict);
            var titleDictionary = LoadTitleMapper(dataFolderPath);
            doiElementDict.Values.ToList().ForEach((v) =>
            {
                if (doiCacheInfoDict.ContainsKey(v.DOI))
                {
                    v.ISBNList.ForEach((isbn) =>
                    {
                        if (isbnDictionary.ContainsKey(isbn))
                        {
                            var containerDOI = isbnDictionary[isbn];
                            if (v.DOI != containerDOI && !doiCacheInfoDict.ContainsKey(containerDOI))
                            {
                                doiCacheInfoDict[containerDOI] = new DOICacheInfo() { DOI = containerDOI, DOIRank = 1 };

                                var cache = doiCacheInfoDict[v.DOI];
                                cache.ContainerDOI = containerDOI;

                            }

                        }

                    });

                }
            });
        }
        */


        /*
                private static void UpdateDummyDOI(string dataFolderPath, IDictionary<string, DOICacheInfo> doiCacheInfoDict)
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
                            if (v.ProperContainerDOI != null && !doiCacheInfoDict.ContainsKey(v.ProperContainerDOI))
                            {
                                var containerTitle = vInfo.ContainerTitle;
                                var containerDOI = vInfo.ContainerDOI;

                                var newDOIElement = new DOIElement() { DOI = containerDOI, Title = containerTitle, Source = "Unknown", IsPrimary = false, Type = "Book?" };
                                dummyDOIElementDict[newDOIElement.DOI] = newDOIElement;
                                doiCacheInfoDict[newDOIElement.DOI] = new DOICacheInfo() { DOI = newDOIElement.DOI, DOIRank = 1, SourceCite = "Dummy", SourceStatus = "Unknown" };
                            }
                            else if (v.ProperContainerDOI == null)
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
                */






        private static Dictionary<string, string> LoadISBNMapper(string dataFolderPath)
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
        private static Dictionary<string, List<string>> LoadTitleMapper(string dataFolderPath)
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
        private static Dictionary<string, string> LoadISSNMapper(string dataFolderPath)
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

        private static void UpdateModifiedTitle(string dataFolderPath, IDictionary<string, DOICacheInfo> doiCacheInfoDict)
        {

            var doiElementDict = CreateDOIElementDictionaryFromSmallCache(dataFolderPath, doiCacheInfoDict);
            var dblpSeriesDictionary = DBLPProceedingsSeriesDictionary.Load(dataFolderPath + "/auto_generated/cache/dblp_cache/dblp_proceedings.jsonl");
            dblpSeriesDictionary.BuildDoiToSeriesTitleAndKeyMapper();

            doiCacheInfoDict.Values.ToList().ForEach((v) =>
            {
                var doiElement = doiElementDict[v.DOI];
                if (doiElement.Type == "ConferencePaper" || doiElement.Type == "proceedings-article")
                {
                    var seriesTitleAndKey = dblpSeriesDictionary.SearchSeriesTitleAndKeyByDOI(doiElement.DOI);
                    if (seriesTitleAndKey != null)
                    {
                        var proceedingsSeries = dblpSeriesDictionary.Series[seriesTitleAndKey.Value.Key];
                        var proceedings = proceedingsSeries.GetProceedings(seriesTitleAndKey.Value.Value);
                        var proceedingsSeriesTitle = proceedingsSeries.SeriesTitle;
                        var proceedingsName = proceedings.Title;

                        var proceedingsSeriesDummyDOI = "dummy/proceedings_series/" + proceedingsSeriesTitle.ToLower();
                        if (!doiCacheInfoDict.ContainsKey(proceedingsSeriesDummyDOI))
                        {
                            doiCacheInfoDict[proceedingsSeriesDummyDOI] = new DOICacheInfo() { DOI = proceedingsSeriesDummyDOI, ModifiedTitle = proceedingsSeriesTitle, DOIRank = 1 };
                        }

                        if (v.ProperContainerDOI.Length > 0)
                        {

                            var proceedingsDOI = v.ProperContainerDOI;
                            //Console.WriteLine("Proceedings DOI: " + proceedingsDOI);
                            if (doiCacheInfoDict.ContainsKey(proceedingsDOI))
                            {
                                var proceedingsCache = doiCacheInfoDict[proceedingsDOI];
                                if (proceedingsCache.ModifiedTitle != proceedingsName && proceedingsCache.ProperContainerDOI != proceedingsSeriesDummyDOI)
                                {
                                    proceedingsCache.ModifiedTitle = proceedingsName;
                                    proceedingsCache.ProperContainerDOI = proceedingsSeriesDummyDOI;

                                    Console.WriteLine("Updated ModifiedTitle: " + proceedingsName + " -> " + proceedingsSeriesTitle);

                                }

                            }
                        }



                        //proceedingsSeries.
                    }
                }

            });
        }



        private static void UpdateContainerDOI(string dataFolderPath, IDictionary<string, DOICacheInfo> doiCacheInfoDict, HashSet<string> crossRefDOIPrefixSet)
        {
            var logFilePath = dataFolderPath + "/auto_generated/log/update_container_doi.log";
            var logFile = new StreamWriter(logFilePath, true);
            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : Start");


            //var filePath = CrossRefDOIToGZFileCache.GetDOIToGZFileFolderPath(dataFolderPath);

            //var titleDictionary = DataCiteMinorCache.LoadTitleFile(dataFolderPath);
            var issnDictionary = LoadISSNMapper(dataFolderPath);
            var isbnDictionary = LoadISBNMapper(dataFolderPath);
            var titleDictionary = LoadTitleMapper(dataFolderPath);

            var doiElementDict = CreateDOIElementDictionaryFromSmallCache(dataFolderPath, doiCacheInfoDict);



            doiCacheInfoDict.Values.ToList().ForEach((w) =>
            {
                w.UpdateContainerDOI(doiElementDict, isbnDictionary, issnDictionary, titleDictionary, logFile);
            });

            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : End");
            logFile.Close();

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
                UpdateModifiedTitle(dataFolderPath, doiCacheInfoDict);
                InsertDOICacheInfoUsingSecondaryDOI(dataFolderPath, doiCacheInfoDict);
                InsertDOICacheInfoUsingContainerDOI(dataFolderPath, doiCacheInfoDict);
                //UpdateDummyDOI(dataFolderPath, doiCacheInfoDict);

                int unknownCounter = doiCacheInfoDict.Values.Count(v => v.SourceStatus == "");
                if (unknownCounter == 0) { break; }
                Console.WriteLine("Waiting for update... " + unknownCounter + " unknown DOIs");

            }

            {
                var doiElementDict = CreateDOIElementDictionaryFromSmallCache(dataFolderPath, doiCacheInfoDict);
                doiElementDict.Values.ToList().ForEach((v) =>
                {
                    if (doiCacheInfoDict.ContainsKey(v.DOI))
                    {
                        var w = doiCacheInfoDict[v.DOI];
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



            DOICacheInfo.Save(doiCacheInfoDict, doiCacheInfoFilePath);
            WriteChecksum(dataFolderPath, checksumFileName, primaryDOISet);

            Console.WriteLine("Building SmallCache [END]");
        }


    }
}