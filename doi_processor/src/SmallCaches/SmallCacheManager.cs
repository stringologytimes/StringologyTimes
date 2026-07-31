using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using System.Text.Json;
using System;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Immutable;
using System.Collections.ObjectModel;


namespace DataProcessor
{
    class SmallCacheManager
    {
        public CrossRefSmallCache CrossRefSmallCache { get; set; } = new CrossRefSmallCache();
        public DataCiteSmallCache DataCiteSmallCache { get; set; } = new DataCiteSmallCache();
        public Dictionary<string, DOIElement> DummyDOIElementDict { get; set; } = new Dictionary<string, DOIElement>();
        public Dictionary<string, SmallCacheSummaryRecord> SmallCacheSummaryRecordDict { get; set; } = new Dictionary<string, SmallCacheSummaryRecord>();

        public StreamWriter SmallCacheSummaryLogFile { get; set; } = new StreamWriter(SmallCacheSummaryRecord.GetSmallCacheSummaryLogPath(), true);

        public SmallCacheManager(string dataFolderPath, ReadOnlySet<string> primaryDOISet)
        {
            CrossRefSmallCache.Load(dataFolderPath);
            DataCiteSmallCache.Load(dataFolderPath);
            DummyDOIElementDict = DOIElement.Load(DummyCacheManager.GetDummyCacheFilePath(dataFolderPath), false);

            var doiCacheInfoFilePath = DOIElementPreprocessor.GetDOICacheInfoPath(dataFolderPath);

            if (new FileInfo(doiCacheInfoFilePath).Exists)
            {
                SmallCacheSummaryRecordDict = SmallCacheSummaryRecord.Load(doiCacheInfoFilePath);
            }


            SmallCacheSummaryRecordDict.Values.ToList().ForEach((v) =>
            {
                v.DOIRank = 1;
            });

            primaryDOISet.ToList().ForEach((v) =>
            {
                if (!SmallCacheSummaryRecordDict.ContainsKey(v))
                {
                    SmallCacheSummaryRecordDict[v] = new SmallCacheSummaryRecord() { DOI = v, DOIRank = 0 };
                    this.DummyDOIElementDict[v] = new DOIElement() { DOI = v, Source = "Unknown", IsPrimary = true };
                    this.SmallCacheSummaryLogFile.WriteLine($"Added Primary DOI in SmallCacheManager: {v}");
                }
                else
                {
                    SmallCacheSummaryRecordDict[v].DOIRank = 0;
                }
            });

            this.CacheConnectionCheck();

        }

        private static void WriteChecksum(string dataFolderPath, string checksumFileName, ReadOnlySet<string> doiSet)
        {
            var currentChecksumDictionary = new Dictionary<string, string>();
            currentChecksumDictionary["doiSet_hash"] = HashFunctions.ComputeHash(doiSet);
            currentChecksumDictionary["date"] = DateTime.Now.ToString("yyyy-MM");
            var checksumFilePath = dataFolderPath + "/auto_generated/cache/" + checksumFileName;
            CSVFunctions.WriteCSVAsDictionary(checksumFilePath, currentChecksumDictionary);
        }

        public void Close(ReadOnlySet<string> primaryDOISet, string checksumFileName)
        {
            DOIElement.Save(DummyDOIElementDict, DummyCacheManager.GetDummyCacheFilePath(Program.DataFolderPath));
            SmallCacheSummaryRecord.Save(SmallCacheSummaryRecordDict, SmallCacheSummaryRecord.GetSmallCacheSummaryFilePath());
            WriteChecksum(Program.DataFolderPath, checksumFileName, primaryDOISet);


            SmallCacheSummaryLogFile.Close();
            SmallCacheSummaryLogFile.Dispose();
        }

        public void MergeCheck()
        {
            var logFilePath = Program.DataFolderPath + "/auto_generated/log/dummy_doi_element_dict.log";
            var logFile = new StreamWriter(logFilePath, true);


            SmallCacheSummaryRecordDict.Values.ToList().ForEach((v) =>
            {
                var doi = v.DOI;
                if (DummyDOIElementDict.ContainsKey(doi))
                {
                    bool b1 = CrossRefSmallCache.localCacheDic.ContainsKey(doi);
                    bool b2 = CrossRefSmallCache.externalCacheDic.ContainsKey(doi);
                    bool b3 = DataCiteSmallCache.localCacheDic.ContainsKey(doi);
                    bool b4 = DataCiteSmallCache.externalCacheDic.ContainsKey(doi);

                    if (b1 || b2 || b3 || b4)
                    {
                        DummyDOIElementDict.Remove(doi);
                        logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : " + doi + " : Removed");
                    }

                }
            });
        }

        public void CacheConnectionCheck()
        {
            SmallCacheSummaryRecordDict.Values.ToList().ForEach((v) =>
            {
                var doi = v.DOI;
                bool b1 = CrossRefSmallCache.localCacheDic.ContainsKey(doi);
                bool b2 = CrossRefSmallCache.externalCacheDic.ContainsKey(doi);
                bool b3 = DataCiteSmallCache.localCacheDic.ContainsKey(doi);
                bool b4 = DataCiteSmallCache.externalCacheDic.ContainsKey(doi);
                bool b5 = DummyDOIElementDict.ContainsKey(doi);

                if (!b1 && !b2 && !b3 && !b4 && !b5)
                {
                    Console.WriteLine("DOI: " + doi + " is not found in any cache");
                    Console.WriteLine("CrossRef Small Cache: " + v.SourceCite);
                    throw new Exception("DOI: " + doi + " is not found in any cache");
                }

            });

        }

        public void UpdateContainerDOI(string dataFolderPath)
        {
            CommonFunctions.OutputSystemMessageFunction("Updating Container DOI [START]");
            CommonFunctions.IncrementParagraphCounter();

            var logFilePath = dataFolderPath + "/auto_generated/log/update_container_doi.log";
            var logFile = new StreamWriter(logFilePath, true);
            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : Start");


            //var filePath = CrossRefDOIToGZFileCache.GetDOIToGZFileFolderPath(dataFolderPath);

            //var titleDictionary = DataCiteMinorCache.LoadTitleFile(dataFolderPath);
            var issnDictionary = DOIElementPreprocessor.LoadISSNMapper(dataFolderPath);
            var isbnDictionary = DOIElementPreprocessor.LoadISBNMapper(dataFolderPath);
            var titleDictionary = DOIElementPreprocessor.LoadTitleMapper(dataFolderPath);

            var doiElementDict = CreateDOIElementDictionaryFromSmallCache(dataFolderPath);



            SmallCacheSummaryRecordDict.Values.ToList().ForEach((w) =>
            {
                w.UpdateContainerDOI(doiElementDict, isbnDictionary, issnDictionary, titleDictionary, logFile);
            });

            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : End");
            logFile.Close();

            CommonFunctions.DecrementParagraphCounter();
            CommonFunctions.OutputSystemMessageFunction("Updating Container DOI [END]");

        }


        public Dictionary<string, DOIElement> CreateDOIElementDictionaryFromSmallCache(string dataFolderPath)
        {
            CommonFunctions.OutputSystemMessageFunction("Creating DOI Element Dictionary From Small Cache [START]");
            CommonFunctions.IncrementParagraphCounter();


            //var crossRefDOIPrefixSet = CrossRefDOIToGZFileCache.GetDOIPrefixSet(dataFolderPath);
            var crossRefdoiElementDict = CrossRefSmallCache.LoadSmallCache(dataFolderPath, SmallCacheSummaryRecordDict);
            var dataCitedoiElementDict = DataCiteSmallCache.LoadSmallCache(dataFolderPath, SmallCacheSummaryRecordDict);

            var mergedDict = new Dictionary<string, DOIElement>();
            crossRefdoiElementDict.ToList().ForEach((v) =>
            {
                mergedDict[v.Key] = v.Value;
            });
            dataCitedoiElementDict.ToList().ForEach((v) =>
            {
                mergedDict[v.Key] = v.Value;
            });

            DummyDOIElementDict.ToList().ForEach((v) =>
            {
                mergedDict[v.Key] = v.Value;
            });

            SmallCacheSummaryRecordDict.Values.ToList().ForEach((v) =>
            {
                if (!mergedDict.ContainsKey(v.DOI))
                {
                    var doiElement = new DOIElement() { DOI = v.DOI, Source = "Unknown", IsPrimary = v.DOIRank == 0 };
                    mergedDict[v.DOI] = doiElement;
                }
            });

            CommonFunctions.DecrementParagraphCounter();
            CommonFunctions.OutputSystemMessageFunction("Creating DOI Element Dictionary From Small Cache [END]");



            return mergedDict;

        }

        public void UpdateTypeByContainer(string dataFolderPath, DBLPProceedingsSeriesDictionary dblpSeriesDictionary)
        {
            CommonFunctions.OutputSystemMessageFunction("Updating Type By Container [START]");
            CommonFunctions.IncrementParagraphCounter();
            var doiElementDict = CreateDOIElementDictionaryFromSmallCache(dataFolderPath);

            SmallCacheSummaryRecordDict.Values.ToList().ForEach((v) =>
                {
                    if (doiElementDict.ContainsKey(v.DOI))
                    {
                        var doiElement = doiElementDict[v.DOI];
                        if (v.ModifiedType.Length == 0 && v.ModifiedContainerDOI.Length > 0 && doiElementDict.ContainsKey(v.ModifiedContainerDOI))
                        {
                            var properContainerDOICacheInfo = SmallCacheSummaryRecordDict[v.ModifiedContainerDOI];
                            if (properContainerDOICacheInfo.ModifiedType == "ConferenceProceeding")
                            {
                                v.ModifiedType = "Proceedings-Article";
                                this.SmallCacheSummaryLogFile.WriteLine($"Updated Type By Container: {v.DOI} -> {v.ModifiedType}");
                            }
                            else if (properContainerDOICacheInfo.ModifiedType == "Book")
                            {
                                v.ModifiedType = "Book-Chapter";
                                this.SmallCacheSummaryLogFile.WriteLine($"Updated Type By Container: {v.DOI} -> {v.ModifiedType}");
                            }
                            else if (properContainerDOICacheInfo.ModifiedType == "ReferenceBook")
                            {
                                v.ModifiedType = "ReferenceBook-Chapter";
                                this.SmallCacheSummaryLogFile.WriteLine($"Updated Type By Container: {v.DOI} -> {v.ModifiedType}");
                            }
                            else if (properContainerDOICacheInfo.ModifiedType == "Monograph")
                            {
                                v.ModifiedType = "Monograph-Chapter";
                                this.SmallCacheSummaryLogFile.WriteLine($"Updated Type By Container: {v.DOI} -> {v.ModifiedType}");
                            }
                        }
                    }



                });

            CommonFunctions.DecrementParagraphCounter();
            CommonFunctions.OutputSystemMessageFunction("Updating Type By Container [END]");
        }

        public void UpdateModifiedTitleUsingDBLP(string dataFolderPath, DBLPProceedingsSeriesDictionary dblpSeriesDictionary)
        {
            CommonFunctions.OutputSystemMessageFunction("Updating Modified Title UsingDBLP [START]");
            CommonFunctions.IncrementParagraphCounter();

            var doiElementDict = CreateDOIElementDictionaryFromSmallCache(dataFolderPath);





            SmallCacheSummaryRecordDict.Values.ToList().ForEach((v) =>
            {
                var doiElement = doiElementDict[v.DOI];





                if (dblpSeriesDictionary.ProceedingsDOIToKeyMapper.ContainsKey(doiElement.DOI) && v.ModifiedType == "" && doiElement.Type != "ConferenceProceeding")
                {
                    var key = dblpSeriesDictionary.ProceedingsDOIToKeyMapper[doiElement.DOI];
                    var proceedings = dblpSeriesDictionary.GetProceedings(key);
                    var proceedingsSeries = dblpSeriesDictionary.Series[proceedings.SeriesTitle];
                    var proceedingsYearAndMonth = SmallCacheSummaryFunctions.ComputeProceedingsYear(proceedings.Year, proceedings.Month, doiElement.Year, doiElement.Month);
                    var proceedingsName = proceedings.SeriesTitle + "(" + proceedingsYearAndMonth.Key + ")";
                    var proceedingsSeriesDummyDOI = DOIFunctions.CreateDummyDOI("proceedings_series", proceedings.SeriesTitle);
                    var (minimum_year, minimum_month) = proceedingsSeries.GetMinimumYearAndMonth();

                    if (!SmallCacheSummaryRecordDict.ContainsKey(proceedingsSeriesDummyDOI) && v.DOIRank == 0)
                    {
                        SmallCacheSummaryFunctions.CreateProceedingsSeriesDummyDOIElement(proceedingsSeriesDummyDOI, proceedings, minimum_year.ToString(), minimum_month.ToString(), this);
                    }

                    if (v.ModifiedType.Length == 0)
                    {
                        v.ModifiedTitle = proceedingsName;
                        v.ModifiedContainerDOI = proceedingsSeriesDummyDOI;
                        v.ModifiedContainerDOIType = "DBLP";
                        v.ModifiedType = "ConferenceProceeding";
                    }



                }


                if (doiElement.IsJournalArticle)
                {
                    SmallCacheSummaryFunctions.UpdateForJournalArticle(doiElement, doiElementDict, this);
                }

                if (doiElement.IsPostedContent)
                {
                    SmallCacheSummaryFunctions.UpdateForPostedContent(doiElement, this);
                }

                if (doiElement.IsPreprint)
                {
                    SmallCacheSummaryFunctions.UpdateForPreprint(doiElement, this);
                }

                if (doiElement.IsProceedingsArticle || doiElement.IsBookChapter)
                {
                    SmallCacheSummaryFunctions.UpdateProcessForProceedingsArticle(doiElement, doiElementDict, this, dblpSeriesDictionary);
                }


            });

            CommonFunctions.DecrementParagraphCounter();
            CommonFunctions.OutputSystemMessageFunction("Updating Modified Title UsingDBLP [END]");
        }

        public void UpdateTypeByCrossRef(string dataFolderPath)
        {
            CommonFunctions.OutputSystemMessageFunction("Modifying Type [START]");
            CommonFunctions.IncrementParagraphCounter();

            var doiElementDict = CreateDOIElementDictionaryFromSmallCache(dataFolderPath);

            var crossRefMapper = new Dictionary<string, string>();
            crossRefMapper["edited-book"] = "EditedBook";
            crossRefMapper["journal-issue"] = "Journal-Issue";
            crossRefMapper["proceedings"] = "Proceedings";
            crossRefMapper["posted-content"] = "PostedContent";
            crossRefMapper["book-chapter"] = "Book-Chapter";
            crossRefMapper["proceedings-article"] = "Proceedings-Article";
            crossRefMapper["book"] = "Book";
            crossRefMapper["reference-book"] = "ReferenceBook";
            crossRefMapper["monograph"] = "Monograph";

            SmallCacheSummaryRecordDict.Values.ToList().ForEach((v) =>
            {
                var doiElement = doiElementDict[v.DOI];
                if (v.ModifiedType.Length == 0)
                {
                    if (doiElement.Source == "CrossRef")
                    {
                        if (crossRefMapper.ContainsKey(doiElement.Type))
                        {
                            v.ModifiedType = crossRefMapper[doiElement.Type];
                            this.SmallCacheSummaryLogFile.WriteLine($"Updated Type By CrossRef: {v.DOI} -> {v.ModifiedType}");
                        }
                    }
                }
            });



            CommonFunctions.DecrementParagraphCounter();
            CommonFunctions.OutputSystemMessageFunction("Modifying Type [END]");
        }


        public void InsertDOICacheInfoUsingSecondaryDOI(string dataFolderPath)
        {
            CommonFunctions.OutputSystemMessageFunction("Updating DOICacheInfo Using Secondary DOI [START]");
            CommonFunctions.IncrementParagraphCounter();

            var doiElementDict = CreateDOIElementDictionaryFromSmallCache(dataFolderPath);
            var doiAliasListMapper = DOIElementPreprocessor.LoadDOIAliasListMapper(dataFolderPath);

            doiElementDict.Values.ToList().ForEach((v) =>
            {
                if (SmallCacheSummaryRecordDict.ContainsKey(v.DOI))
                {
                    var w = SmallCacheSummaryRecordDict[v.DOI];

                    if (w.DOIRank == 0)
                    {
                        if (w.ModifiedContainerDOI.Length > 0 && !SmallCacheSummaryRecordDict.ContainsKey(w.ModifiedContainerDOI))
                        {
                            SmallCacheSummaryRecordDict[w.ModifiedContainerDOI] = new SmallCacheSummaryRecord() { DOI = w.ModifiedContainerDOI, DOIRank = 1 };
                            this.DummyDOIElementDict[w.ModifiedContainerDOI] = new DOIElement() { DOI = w.ModifiedContainerDOI, Source = "Unknown", IsPrimary = false };
                            this.SmallCacheSummaryLogFile.WriteLine($"Added Dummy DOI by ModifiedContainerDOI in InsertDOICacheInfoUsingSecondaryDOI: {w.ModifiedContainerDOI}");
                        }


                        v.DOIReferences.ForEach((referenceDOI) =>
                        {
                            referenceDOI = doiAliasListMapper.ContainsKey(referenceDOI) ? doiAliasListMapper[referenceDOI] : referenceDOI;

                            if (!SmallCacheSummaryRecordDict.ContainsKey(referenceDOI))
                            {
                                SmallCacheSummaryRecordDict[referenceDOI] = new SmallCacheSummaryRecord() { DOI = referenceDOI, DOIRank = 1 };
                                this.DummyDOIElementDict[referenceDOI] = new DOIElement() { DOI = referenceDOI, Source = "Unknown", IsPrimary = false };
                                this.SmallCacheSummaryLogFile.WriteLine($"Added Dummy DOI by referenceDOI in InsertDOICacheInfoUsingSecondaryDOI: {referenceDOI}");

                            }
                        });

                    }

                }



            });

            CommonFunctions.DecrementParagraphCounter();
            CommonFunctions.OutputSystemMessageFunction("Updating DOICacheInfo Using Secondary DOI [END]");
        }


        /*
                public void InsertDOICacheInfoUsingContainerDOI(string dataFolderPath)
                {
                    CommonFunctions.OutputSystemMessageFunction("Updating DOICacheInfo Using Container DOI [START]");
                    CommonFunctions.IncrementParagraphCounter();

                    var doiElementDict = CreateDOIElementDictionaryFromSmallCache(dataFolderPath);
                    var isbnDictionary = DOIElementPreprocessor.LoadISBNMapper(dataFolderPath);
                    doiElementDict.Values.ToList().ForEach((v) =>
                    {
                        if (DOICacheInfoDict.ContainsKey(v.DOI))
                        {
                            var w = DOICacheInfoDict[v.DOI];
                            if (w.ModifiedContainerDOI.Length > 0 && !DOICacheInfoDict.ContainsKey(w.ModifiedContainerDOI))
                            {
                                DOICacheInfoDict[w.ModifiedContainerDOI] = new DOICacheInfo() { DOI = w.ModifiedContainerDOI, DOIRank = 1 };
                            }


                        }
                    });

                    CommonFunctions.DecrementParagraphCounter();
                    CommonFunctions.OutputSystemMessageFunction("Updating DOICacheInfo Using Container DOI [END]");
                }
                */


    }
}