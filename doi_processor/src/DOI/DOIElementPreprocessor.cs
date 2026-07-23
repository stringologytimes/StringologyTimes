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
        public static Dictionary<string, DOIElement> CreateDOIElementDictionaryFromSmallCache(string dataFolderPath,
        IDictionary<string, DOICacheInfo> doiCacheInfoDict, CrossRefSmallCache crossRefSmallCache, DataCiteSmallCache dataCiteSmallCache, Dictionary<string, DOIElement> dummyDOIElementDict)
        {


            //var crossRefDOIPrefixSet = CrossRefDOIToGZFileCache.GetDOIPrefixSet(dataFolderPath);
            var crossRefdoiElementDict = crossRefSmallCache.LoadSmallCache(dataFolderPath, doiCacheInfoDict);
            var dataCitedoiElementDict = dataCiteSmallCache.LoadSmallCache(dataFolderPath, doiCacheInfoDict);

            var mergedDict = new Dictionary<string, DOIElement>();
            crossRefdoiElementDict.ToList().ForEach((v) =>
            {
                mergedDict[v.Key] = v.Value;
            });
            dataCitedoiElementDict.ToList().ForEach((v) =>
            {
                mergedDict[v.Key] = v.Value;
            });

            dummyDOIElementDict.ToList().ForEach((v) =>
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

        private static void InsertDOICacheInfoUsingSecondaryDOI(string dataFolderPath, IDictionary<string, DOICacheInfo> doiCacheInfoDict, CrossRefSmallCache crossRefSmallCache, DataCiteSmallCache dataCiteSmallCache, Dictionary<string, DOIElement> dummyDOIElementDict)
        {
            CommonFunctions.OutputSystemMessageFunction("Updating DOICacheInfo Using Secondary DOI [START]");
            CommonFunctions.IncrementParagraphCounter();

            var doiElementDict = CreateDOIElementDictionaryFromSmallCache(dataFolderPath, doiCacheInfoDict, crossRefSmallCache, dataCiteSmallCache, dummyDOIElementDict);
            var doiAliasListMapper = LoadDOIAliasListMapper(dataFolderPath);

            doiElementDict.Values.ToList().ForEach((v) =>
            {
                if (doiCacheInfoDict.ContainsKey(v.DOI))
                {
                    var w = doiCacheInfoDict[v.DOI];
                    if (w.ProperContainerDOI.Length > 0 && !doiCacheInfoDict.ContainsKey(w.ProperContainerDOI))
                    {
                        doiCacheInfoDict[w.ProperContainerDOI] = new DOICacheInfo() { DOI = w.ProperContainerDOI, DOIRank = 1 };
                    }

                    if (w.DOIRank == 0)
                    {

                        v.DOIReferences.ForEach((referenceDOI) =>
                        {
                            referenceDOI = doiAliasListMapper.ContainsKey(referenceDOI) ? doiAliasListMapper[referenceDOI] : referenceDOI;

                            if (!doiCacheInfoDict.ContainsKey(referenceDOI))
                            {
                                doiCacheInfoDict[referenceDOI] = new DOICacheInfo() { DOI = referenceDOI, DOIRank = 1 };
                            }
                        });

                    }

                }



            });

            CommonFunctions.DecrementParagraphCounter();
            CommonFunctions.OutputSystemMessageFunction("Updating DOICacheInfo Using Secondary DOI [END]");
        }
        private static void InsertDOICacheInfoUsingContainerDOI(string dataFolderPath, IDictionary<string, DOICacheInfo> doiCacheInfoDict, CrossRefSmallCache crossRefSmallCache, DataCiteSmallCache dataCiteSmallCache, Dictionary<string, DOIElement> dummyDOIElementDict)
        {
            CommonFunctions.OutputSystemMessageFunction("Updating DOICacheInfo Using Container DOI [START]");
            CommonFunctions.IncrementParagraphCounter();

            var doiElementDict = CreateDOIElementDictionaryFromSmallCache(dataFolderPath, doiCacheInfoDict, crossRefSmallCache, dataCiteSmallCache, dummyDOIElementDict);
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


                }
            });

            CommonFunctions.DecrementParagraphCounter();
            CommonFunctions.OutputSystemMessageFunction("Updating DOICacheInfo Using Container DOI [END]");
        }



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

        private static void CreateProceedingsSeriesDummyDOIElement(IDictionary<string, DOIElement> dummyDOIElementDict, IDictionary<string, DOICacheInfo> doiCacheInfoDict, string proceedingsSeriesDummyDOI, DBLPProceedings proceedings, string minimum_year, string minimum_month)
        {
            var proceedingsSeriesDummyDOIElement = new DOIElement()
            {
                DOI = proceedingsSeriesDummyDOI,
                Title = proceedings.SeriesTitle,
                Source = "DUMMY",
                IsPrimary = false,
                Type = "ProceedingsSeries",
                ContainerDOI = "",
                Year = minimum_year.ToString(),
                Month = minimum_month.ToString()
            };


            if (!dummyDOIElementDict.ContainsKey(proceedingsSeriesDummyDOI))
            {
                dummyDOIElementDict[proceedingsSeriesDummyDOI] = proceedingsSeriesDummyDOIElement;
            }

            doiCacheInfoDict[proceedingsSeriesDummyDOI] = new DOICacheInfo()
            {
                DOI = proceedingsSeriesDummyDOI,
                ModifiedTitle = proceedings.SeriesTitle,
                DOIRank = 1
            };

        }

        private static void CreateProceedingsDummyDOIElement(IDictionary<string, DOIElement> dummyDOIElementDict, IDictionary<string, DOICacheInfo> doiCacheInfoDict, string proceedingsDOI, string proceedingsName, string containerDOI)
        {
            var proceedingsDummyDOIElement = new DOIElement()
            {
                DOI = proceedingsDOI,
                Title = proceedingsName,
                Source = "DUMMY",
                IsPrimary = false,
                Type = "Proceedings",
                ContainerDOI = containerDOI
            };

            if (!dummyDOIElementDict.ContainsKey(proceedingsDOI))
            {
                dummyDOIElementDict[proceedingsDOI] = proceedingsDummyDOIElement;
            }

            doiCacheInfoDict[proceedingsDOI] = new DOICacheInfo()
            {
                DOI = proceedingsDOI,
                ModifiedTitle = proceedingsName,
                DOIRank = 1
            };
        }

        private static void CreateJournalDummyDOIElement(IDictionary<string, DOIElement> dummyDOIElementDict, IDictionary<string, DOICacheInfo> doiCacheInfoDict, string journalDOI, string journalTitle)
        {
            var journalDummyDOIElement = new DOIElement()
            {
                DOI = journalDOI,
                Title = journalTitle,
                Source = "DUMMY",
                IsPrimary = false,
                Type = "Journal",
                ContainerDOI = ""
            };


            if (!dummyDOIElementDict.ContainsKey(journalDOI))
            {
                dummyDOIElementDict[journalDOI] = journalDummyDOIElement;
            }

            doiCacheInfoDict[journalDOI] = new DOICacheInfo()
            {
                DOI = journalDOI,
                ModifiedTitle = journalTitle,
                DOIRank = 1
            };

        }

        private static void CreateJournalIssueDummyDOIElement(IDictionary<string, DOIElement> dummyDOIElementDict,
        IDictionary<string, DOICacheInfo> doiCacheInfoDict, string journalIssueDummyDOI, string journalIssueTitle, string containerDOI)
        {
            var journalIssueDummyDOIElement = new DOIElement()
            {
                DOI = journalIssueDummyDOI,
                Title = journalIssueTitle,
                Source = "DUMMY",
                IsPrimary = false,
                Type = "Journal-Issue",
                ContainerDOI = containerDOI
            };

            if (!dummyDOIElementDict.ContainsKey(journalIssueDummyDOI))
            {
                dummyDOIElementDict[journalIssueDummyDOI] = journalIssueDummyDOIElement;
            }

            doiCacheInfoDict[journalIssueDummyDOI] = new DOICacheInfo()
            {
                DOI = journalIssueDummyDOI,
                ModifiedTitle = journalIssueTitle,
                DOIRank = 1
            };
        }



        private static void UpdateModifiedTitleUsingDBLP(string dataFolderPath, IDictionary<string, DOICacheInfo> doiCacheInfoDict,
        CrossRefSmallCache crossRefSmallCache, DataCiteSmallCache dataCiteSmallCache, Dictionary<string, DOIElement> dummyDOIElementDict, DBLPProceedingsSeriesDictionary dblpSeriesDictionary)
        {
            CommonFunctions.OutputSystemMessageFunction("Updating Modified Title UsingDBLP [START]");
            CommonFunctions.IncrementParagraphCounter();

            var doiElementDict = CreateDOIElementDictionaryFromSmallCache(dataFolderPath, doiCacheInfoDict, crossRefSmallCache, dataCiteSmallCache, dummyDOIElementDict);



            doiCacheInfoDict.Values.ToList().ForEach((v) =>
            {
                var doiElement = doiElementDict[v.DOI];




                if (dblpSeriesDictionary.ProceedingsDOIToKeyMapper.ContainsKey(doiElement.DOI) && v.ModifiedType == "" && doiElement.Type != "ConferenceProceeding")
                {
                    var key = dblpSeriesDictionary.ProceedingsDOIToKeyMapper[doiElement.DOI];
                    var proceedings = dblpSeriesDictionary.GetProceedings(key);
                    var proceedingsSeries = dblpSeriesDictionary.Series[proceedings.SeriesTitle];
                    var proceedingsName = proceedings.SeriesTitle + "(" + proceedings.Year + ")";
                    var proceedingsSeriesDummyDOI = DOIFunctions.CreateDummyDOI("proceedings_series", proceedings.SeriesTitle);
                    var (minimum_year, minimum_month) = proceedingsSeries.GetMinimumYearAndMonth();

                    if (!doiCacheInfoDict.ContainsKey(proceedingsSeriesDummyDOI))
                    {
                        CreateProceedingsSeriesDummyDOIElement(dummyDOIElementDict, doiCacheInfoDict, proceedingsSeriesDummyDOI, proceedings, minimum_year.ToString(), minimum_month.ToString());
                    }


                    v.ModifiedTitle = proceedingsName;
                    v.ProperContainerDOI = proceedingsSeriesDummyDOI;
                    v.ProperContainerDOIType = "DBLP";
                    v.ModifiedType = "ConferenceProceeding";

                }

                if (v.ModifiedType.Length == 0)
                {
                    if (doiElement.Type == "book")
                    {
                        v.ModifiedType = "Book";
                    }
                    else if (doiElement.Type == "reference-book")
                    {
                        v.ModifiedType = "ReferenceBook";
                    }
                    else if (doiElement.Type == "monograph")
                    {
                        v.ModifiedType = "Monograph";
                    }
                }

                if (doiElement.Type == "journal-article")
                {
                    var journalTitle = doiElement.ContainerTitle;
                    var volumeIssueString = doiElement.GetVolumeIssueString();
                    var journalDOI = doiElement.ContainerDOI;
                    if (journalDOI.Length == 0)
                    {
                        journalDOI = DOIFunctions.CreateDummyDOI("journal", journalTitle);
                    }

                    if (!doiCacheInfoDict.ContainsKey(journalDOI))
                    {
                        CreateJournalDummyDOIElement(dummyDOIElementDict, doiCacheInfoDict, journalDOI, journalTitle);
                    }

                    var journalIssueTitle = journalTitle + "(" + volumeIssueString + ")";
                    var journalIssueDummyDOI = DOIFunctions.CreateDummyDOI("journal_issue", journalIssueTitle);

                    if (!doiCacheInfoDict.ContainsKey(journalIssueDummyDOI))
                    {
                        CreateJournalIssueDummyDOIElement(dummyDOIElementDict, doiCacheInfoDict, journalIssueDummyDOI, journalIssueTitle, journalDOI);
                    }

                    if (v.ProperContainerDOI.Length == 0)
                    {
                        v.ProperContainerDOI = journalIssueDummyDOI;
                        v.ProperContainerDOIType = "Metadata";
                        v.ModifiedType = "Journal-Article";
                    }



                }




                if (doiElement.Type == "ConferencePaper" || doiElement.Type == "proceedings-article" || doiElement.Type == "book-chapter")
                {
                    var seriesTitleAndKey = dblpSeriesDictionary.SearchSeriesTitleAndKeyByDOI(doiElement.DOI);



                    if (seriesTitleAndKey != null)
                    {
                        if (doiElement.DOI == "10.1109/ccp.2011.45")
                        {
                            CommonFunctions.OutputSystemMessageFunction("Proceedings DOI: " + doiElement.DOI + " : " + seriesTitleAndKey.Value.Key + " : " + seriesTitleAndKey.Value.Value, ConsoleColor.Red);
                            CommonFunctions.OutputSystemMessageFunction("Proceedings Name: " + v.ProperContainerDOI, ConsoleColor.Red);
                        }



                        var proceedingsSeries = dblpSeriesDictionary.Series[seriesTitleAndKey.Value.Key];
                        var proceedings = proceedingsSeries.GetProceedings(seriesTitleAndKey.Value.Value);
                        var proceedingsSeriesTitle = proceedingsSeries.SeriesTitle;
                        var proceedingsName = proceedings.SeriesTitle + "(" + proceedings.Year + ")";
                        var proceedingsDOI = doiElement.ContainerDOI;
                        if (proceedingsDOI.Length == 0)
                        {
                            proceedingsDOI = proceedings.DOI.Length > 0 ? proceedings.DOI : DOIFunctions.CreateDummyDOI("proceedings", proceedingsName);
                        }




                        var (minimum_year, minimum_month) = proceedingsSeries.GetMinimumYearAndMonth();


                        var proceedingsSeriesDummyDOI = DOIFunctions.CreateDummyDOI("proceedings_series", proceedingsSeriesTitle);
                        if (!doiCacheInfoDict.ContainsKey(proceedingsSeriesDummyDOI))
                        {
                            CreateProceedingsSeriesDummyDOIElement(dummyDOIElementDict, doiCacheInfoDict, proceedingsSeriesDummyDOI, proceedings, minimum_year.ToString(), minimum_month.ToString());
                        }

                        if (!doiCacheInfoDict.ContainsKey(proceedingsDOI))
                        {
                            CreateProceedingsDummyDOIElement(dummyDOIElementDict, doiCacheInfoDict, proceedingsDOI, proceedingsName, proceedingsSeriesDummyDOI);
                        }




                        if (v.ProperContainerDOI.Length > 0)
                        {

                            //var proceedingsDOI = v.ProperContainerDOI;
                            //Console.WriteLine("Proceedings DOI: " + proceedingsDOI);
                            if (doiCacheInfoDict.ContainsKey(proceedingsDOI))
                            {
                                var proceedingsCache = doiCacheInfoDict[proceedingsDOI];
                                if (proceedingsCache.ModifiedTitle != proceedingsName && proceedingsCache.ProperContainerDOI != proceedingsSeriesDummyDOI)
                                {
                                    proceedingsCache.ModifiedTitle = proceedingsName;
                                    proceedingsCache.ProperContainerDOI = proceedingsSeriesDummyDOI;
                                    proceedingsCache.ModifiedType = "ConferenceProceeding";
                                    proceedingsCache.ProperContainerDOIType = "DBLP";
                                }

                            }
                        }
                        else
                        {
                            v.ProperContainerDOI = proceedingsDOI;
                            v.ProperContainerDOIType = "DBLP";
                            v.ModifiedType = "Proceedings-Article";



                        }




                        //proceedingsSeries.
                    }

                }


            });

            doiCacheInfoDict.Values.ToList().ForEach((v) =>
                {
                    if (doiElementDict.ContainsKey(v.DOI))
                    {
                        var doiElement = doiElementDict[v.DOI];
                        if (v.ModifiedType.Length == 0 && v.ProperContainerDOI.Length > 0 && doiElementDict.ContainsKey(v.ProperContainerDOI))
                        {
                            var properContainerDOICacheInfo = doiCacheInfoDict[v.ProperContainerDOI];
                            if (properContainerDOICacheInfo.ModifiedType == "ConferenceProceeding")
                            {
                                v.ModifiedType = "Proceedings-Article";
                            }
                            else if (properContainerDOICacheInfo.ModifiedType == "Book")
                            {
                                v.ModifiedType = "Book-Chapter";
                            }
                            else if (properContainerDOICacheInfo.ModifiedType == "ReferenceBook")
                            {
                                v.ModifiedType = "ReferenceBook-Chapter";
                            }
                            else if (properContainerDOICacheInfo.ModifiedType == "Monograph")
                            {
                                v.ModifiedType = "Monograph-Chapter";
                            }
                        }
                    }



                });


            DOIElement.Save(dummyDOIElementDict, DummyCacheManager.GetDummyCacheFilePath(dataFolderPath));

            CommonFunctions.DecrementParagraphCounter();
            CommonFunctions.OutputSystemMessageFunction("Updating Modified Title UsingDBLP [END]");
        }



        private static void UpdateContainerDOI(string dataFolderPath, IDictionary<string, DOICacheInfo> doiCacheInfoDict, CrossRefSmallCache crossRefSmallCache, DataCiteSmallCache dataCiteSmallCache, Dictionary<string, DOIElement> dummyDOIElementDict)
        {
            CommonFunctions.OutputSystemMessageFunction("Updating Container DOI [START]");
            CommonFunctions.IncrementParagraphCounter();

            var logFilePath = dataFolderPath + "/auto_generated/log/update_container_doi.log";
            var logFile = new StreamWriter(logFilePath, true);
            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : Start");


            //var filePath = CrossRefDOIToGZFileCache.GetDOIToGZFileFolderPath(dataFolderPath);

            //var titleDictionary = DataCiteMinorCache.LoadTitleFile(dataFolderPath);
            var issnDictionary = LoadISSNMapper(dataFolderPath);
            var isbnDictionary = LoadISBNMapper(dataFolderPath);
            var titleDictionary = LoadTitleMapper(dataFolderPath);

            var doiElementDict = CreateDOIElementDictionaryFromSmallCache(dataFolderPath, doiCacheInfoDict, crossRefSmallCache, dataCiteSmallCache, dummyDOIElementDict);



            doiCacheInfoDict.Values.ToList().ForEach((w) =>
            {
                w.UpdateContainerDOI(doiElementDict, isbnDictionary, issnDictionary, titleDictionary, logFile);
            });

            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : End");
            logFile.Close();

            CommonFunctions.DecrementParagraphCounter();
            CommonFunctions.OutputSystemMessageFunction("Updating Container DOI [END]");

        }

        private static async Task MainLoop(string dataFolderPath, string mailAddress, IDictionary<string, DOICacheInfo> doiCacheInfoDict, HashSet<string> crossRefDOIPrefixSet, HashSet<string> dataCiteDOIPrefixSet)
        {
            var round = 0;

            var crossRefSmallCache = new CrossRefSmallCache();
            crossRefSmallCache.Load(dataFolderPath);
            var dataCiteSmallCache = new DataCiteSmallCache();
            dataCiteSmallCache.Load(dataFolderPath);

            var dummyDOIElementDict = DOIElement.Load(DummyCacheManager.GetDummyCacheFilePath(dataFolderPath), false);

            var dblpSeriesDictionary = DBLPProceedingsSeriesDictionary.Load(dataFolderPath + "/auto_generated/cache/dblp_cache/dblp_proceedings.jsonl");
            dblpSeriesDictionary.BuildDoiToSeriesTitleAndKeyMapper();



            CommonFunctions.OutputSystemMessageFunction("Building SmallCache [START]");
            CommonFunctions.IncrementParagraphCounter();
            while (true)
            {
                round++;
                CommonFunctions.OutputSystemMessageFunction("Round: " + round, ConsoleColor.Green);
                foreach (var v in doiCacheInfoDict.Values)
                {
                    if (v.SourceCite.Length == 0)
                    {
                        v.UpdateSourceCite(crossRefDOIPrefixSet, dataCiteDOIPrefixSet);
                    }
                }


                await DataProcessor.CrossRefCacheBuilder.UpdateSmallCache(dataFolderPath, doiCacheInfoDict, crossRefSmallCache, mailAddress);
                await DataProcessor.DataCitePreprocessor.UpdateSmallCache(dataFolderPath, doiCacheInfoDict, dataCiteSmallCache, mailAddress);
                UpdateContainerDOI(dataFolderPath, doiCacheInfoDict, crossRefSmallCache, dataCiteSmallCache, dummyDOIElementDict);
                UpdateModifiedTitleUsingDBLP(dataFolderPath, doiCacheInfoDict, crossRefSmallCache, dataCiteSmallCache, dummyDOIElementDict, dblpSeriesDictionary);
                InsertDOICacheInfoUsingSecondaryDOI(dataFolderPath, doiCacheInfoDict, crossRefSmallCache, dataCiteSmallCache, dummyDOIElementDict);
                InsertDOICacheInfoUsingContainerDOI(dataFolderPath, doiCacheInfoDict, crossRefSmallCache, dataCiteSmallCache, dummyDOIElementDict);
                //UpdateDummyDOI(dataFolderPath, doiCacheInfoDict);

                int unknownCounter = doiCacheInfoDict.Values.Count(v => v.SourceStatus == "");
                if (unknownCounter == 0) { break; }
                Console.WriteLine("Waiting for update... " + unknownCounter + " unknown DOIs");

            }


            {
                var doiElementDict = CreateDOIElementDictionaryFromSmallCache(dataFolderPath, doiCacheInfoDict, crossRefSmallCache, dataCiteSmallCache, dummyDOIElementDict);
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


            CommonFunctions.DecrementParagraphCounter();
            CommonFunctions.OutputSystemMessageFunction("Building SmallCache [END]");





            crossRefSmallCache.Save(dataFolderPath);
            dataCiteSmallCache.Save(dataFolderPath);
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

            await MainLoop(dataFolderPath, mailAddress, doiCacheInfoDict, crossRefDOIPrefixSet, dataCiteDOIPrefixSet);







            DOICacheInfo.Save(doiCacheInfoDict, doiCacheInfoFilePath);
            WriteChecksum(dataFolderPath, checksumFileName, primaryDOISet);

            Console.WriteLine("Building SmallCache [END]");
        }


    }
}