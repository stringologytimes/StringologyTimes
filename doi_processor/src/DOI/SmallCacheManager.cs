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
        public Dictionary<string, DOICacheInfo> DOICacheInfoDict { get; set; } = new Dictionary<string, DOICacheInfo>();

        public SmallCacheManager(string dataFolderPath, ReadOnlySet<string> primaryDOISet)
        {
            CrossRefSmallCache.Load(dataFolderPath);
            DataCiteSmallCache.Load(dataFolderPath);
            DummyDOIElementDict = DOIElement.Load(DummyCacheManager.GetDummyCacheFilePath(dataFolderPath), false);

            var doiCacheInfoFilePath = DOIElementPreprocessor.GetDOICacheInfoPath(dataFolderPath);

            if (new FileInfo(doiCacheInfoFilePath).Exists)
            {
                DOICacheInfoDict = DOICacheInfo.Load(doiCacheInfoFilePath);
            }


            DOICacheInfoDict.Values.ToList().ForEach((v) =>
            {
                v.DOIRank = 1;
            });

            primaryDOISet.ToList().ForEach((v) =>
            {
                if (!DOICacheInfoDict.ContainsKey(v))
                {
                    DOICacheInfoDict[v] = new DOICacheInfo() { DOI = v, DOIRank = 0 };
                    this.DummyDOIElementDict[v] = new DOIElement() { DOI = v, Source = "Unknown", IsPrimary = true };
                }
                else
                {
                    DOICacheInfoDict[v].DOIRank = 0;
                }
            });

            this.CacheConnectionCheck();

        }

        public void MergeCheck()
        {
            var logFilePath = Program.DataFolderPath + "/auto_generated/log/dummy_doi_element_dict.log";
            var logFile = new StreamWriter(logFilePath, true);


            DOICacheInfoDict.Values.ToList().ForEach((v) =>
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
            DOICacheInfoDict.Values.ToList().ForEach((v) =>
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



            DOICacheInfoDict.Values.ToList().ForEach((w) =>
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
            var crossRefdoiElementDict = CrossRefSmallCache.LoadSmallCache(dataFolderPath, DOICacheInfoDict);
            var dataCitedoiElementDict = DataCiteSmallCache.LoadSmallCache(dataFolderPath, DOICacheInfoDict);

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

            DOICacheInfoDict.Values.ToList().ForEach((v) =>
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

        public void UpdateModifiedTitleUsingDBLP(string dataFolderPath, DBLPProceedingsSeriesDictionary dblpSeriesDictionary)
        {
            CommonFunctions.OutputSystemMessageFunction("Updating Modified Title UsingDBLP [START]");
            CommonFunctions.IncrementParagraphCounter();

            var doiElementDict = CreateDOIElementDictionaryFromSmallCache(dataFolderPath);





            DOICacheInfoDict.Values.ToList().ForEach((v) =>
            {
                var doiElement = doiElementDict[v.DOI];





                if (dblpSeriesDictionary.ProceedingsDOIToKeyMapper.ContainsKey(doiElement.DOI) && v.ModifiedType == "" && doiElement.Type != "ConferenceProceeding")
                {
                    var key = dblpSeriesDictionary.ProceedingsDOIToKeyMapper[doiElement.DOI];
                    var proceedings = dblpSeriesDictionary.GetProceedings(key);
                    var proceedingsSeries = dblpSeriesDictionary.Series[proceedings.SeriesTitle];
                    var proceedingsYear = DOICacheInfoFunctions.ComputeProceedingsYear(proceedings.Year, doiElement.Year);
                    var proceedingsName = proceedings.SeriesTitle + "(" + proceedingsYear + ")";
                    var proceedingsSeriesDummyDOI = DOIFunctions.CreateDummyDOI("proceedings_series", proceedings.SeriesTitle);
                    var (minimum_year, minimum_month) = proceedingsSeries.GetMinimumYearAndMonth();

                    if (!DOICacheInfoDict.ContainsKey(proceedingsSeriesDummyDOI))
                    {
                        DOICacheInfoFunctions.CreateProceedingsSeriesDummyDOIElement(DummyDOIElementDict, DOICacheInfoDict, proceedingsSeriesDummyDOI, proceedings, minimum_year.ToString(), minimum_month.ToString());
                    }



                    v.ModifiedTitle = proceedingsName;
                    v.ModifiedContainerDOI = proceedingsSeriesDummyDOI;
                    v.ModifiedContainerDOIType = "DBLP";
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

                    if (!DOICacheInfoDict.ContainsKey(journalDOI))
                    {
                        DOICacheInfoFunctions.CreateJournalDummyDOIElement(doiElementDict, DummyDOIElementDict, DOICacheInfoDict, journalDOI, journalTitle);
                    }

                    var journalIssueTitle = journalTitle + "(" + volumeIssueString + ")";
                    var journalIssueDummyDOI = DOIFunctions.CreateDummyDOI("journal_issue", journalIssueTitle);

                    if (!DOICacheInfoDict.ContainsKey(journalIssueDummyDOI))
                    {
                        DOICacheInfoFunctions.CreateJournalIssueDummyDOIElement(DummyDOIElementDict, DOICacheInfoDict, journalIssueDummyDOI, journalIssueTitle, journalDOI);
                    }

                    if (v.ModifiedContainerDOI.Length == 0)
                    {
                        v.ModifiedContainerDOI = journalIssueDummyDOI;
                        v.ModifiedContainerDOIType = "Metadata";
                        v.ModifiedType = "Journal-Article";
                    }
                }

                if (doiElement.Type == "posted-content")
                {
                    if (v.ModifiedType != "Preprint" && doiElement.IdentifierTypeOrInstitution == "bioRxiv")
                    {
                        v.ModifiedType = "Preprint";

                        var bioRxivDOI = DOIFunctions.CreateDummyDOI("preprint_repository", "biorxiv");
                        if (!DOICacheInfoDict.ContainsKey(bioRxivDOI))
                        {
                            DOICacheInfoFunctions.CreatePreprintRepositoryDummyDOIElement(DummyDOIElementDict, DOICacheInfoDict, bioRxivDOI, "bioRxiv");
                        }

                        v.ModifiedContainerDOI = bioRxivDOI;
                        v.ModifiedContainerDOIType = "Metadata";

                    }

                }

                if (doiElement.Type == "Preprint")
                {
                    if (v.ModifiedType != "Preprint" && doiElement.IdentifierTypeOrInstitution.Length > 0)
                    {
                        var preprintRepositoryDOI = DOIFunctions.CreateDummyDOI("preprint_repository", doiElement.IdentifierTypeOrInstitution);
                        if (!DOICacheInfoDict.ContainsKey(preprintRepositoryDOI))
                        {
                            DOICacheInfoFunctions.CreatePreprintRepositoryDummyDOIElement(DummyDOIElementDict, DOICacheInfoDict, preprintRepositoryDOI, doiElement.IdentifierTypeOrInstitution);
                        }

                        v.ModifiedContainerDOI = preprintRepositoryDOI;
                        v.ModifiedContainerDOIType = "Metadata";
                        v.ModifiedType = "Preprint";
                    }
                }




                if (doiElement.Type == "ConferencePaper" || doiElement.Type == "proceedings-article" || doiElement.Type == "book-chapter")
                {
                    var seriesTitleAndKey = dblpSeriesDictionary.SearchSeriesTitleAndKeyByDOI(doiElement.DOI);



                    if (seriesTitleAndKey != null)
                    {
                        if (!dblpSeriesDictionary.Series.ContainsKey(seriesTitleAndKey.Value.Key))
                        {
                            throw new Exception("Series Title and Key: " + seriesTitleAndKey.Value.Key + " is not found in dblpSeriesDictionary.Series");


                        }


                        var proceedingsSeries = dblpSeriesDictionary.Series[seriesTitleAndKey.Value.Key];
                        var proceedings = proceedingsSeries.GetProceedings(seriesTitleAndKey.Value.Value);
                        var proceedingsSeriesTitle = proceedingsSeries.SeriesTitle;
                        var proceedingsYear = DOICacheInfoFunctions.ComputeProceedingsYear(proceedings.Year, doiElement.Year);
                        var proceedingsName = proceedings.SeriesTitle + "(" + proceedingsYear + ")";




                        var proceedingsDOI = doiElement.ContainerDOI;
                        if (proceedingsDOI.Length == 0)
                        {
                            proceedingsDOI = proceedings.DOI.Length > 0 ? proceedings.DOI : DOIFunctions.CreateDummyDOI("proceedings", proceedingsName);
                        }





                        var (minimum_year, minimum_month) = proceedingsSeries.GetMinimumYearAndMonth();


                        var proceedingsSeriesDummyDOI = DOIFunctions.CreateDummyDOI("proceedings_series", proceedingsSeriesTitle);
                        if (!DOICacheInfoDict.ContainsKey(proceedingsSeriesDummyDOI))
                        {
                            DOICacheInfoFunctions.CreateProceedingsSeriesDummyDOIElement(DummyDOIElementDict, DOICacheInfoDict, proceedingsSeriesDummyDOI, proceedings, minimum_year.ToString(), minimum_month.ToString());
                        }

                        if (!DOICacheInfoDict.ContainsKey(proceedingsDOI))
                        {
                            DOICacheInfoFunctions.CreateProceedingsDummyDOIElement(doiElementDict, DummyDOIElementDict, DOICacheInfoDict, proceedingsDOI, proceedingsName, proceedingsSeriesDummyDOI);
                        }




                        if (v.ModifiedContainerDOI.Length > 0)
                        {

                            //var proceedingsDOI = v.ProperContainerDOI;
                            //Console.WriteLine("Proceedings DOI: " + proceedingsDOI);
                            if (DOICacheInfoDict.ContainsKey(proceedingsDOI))
                            {
                                var proceedingsCache = DOICacheInfoDict[proceedingsDOI];
                                if (proceedingsCache.ModifiedTitle != proceedingsName && proceedingsCache.ModifiedContainerDOI != proceedingsSeriesDummyDOI)
                                {
                                    proceedingsCache.ModifiedTitle = proceedingsName;
                                    proceedingsCache.ModifiedContainerDOI = proceedingsSeriesDummyDOI;
                                    proceedingsCache.ModifiedType = "ConferenceProceeding";
                                    proceedingsCache.ModifiedContainerDOIType = "DBLP";
                                }

                            }
                        }
                        else
                        {
                            v.ModifiedContainerDOI = proceedingsDOI;
                            v.ModifiedContainerDOIType = "DBLP";
                            v.ModifiedType = "Proceedings-Article";



                        }




                        //proceedingsSeries.
                    }

                }


            });

            DOICacheInfoDict.Values.ToList().ForEach((v) =>
                {
                    if (doiElementDict.ContainsKey(v.DOI))
                    {
                        var doiElement = doiElementDict[v.DOI];
                        if (v.ModifiedType.Length == 0 && v.ModifiedContainerDOI.Length > 0 && doiElementDict.ContainsKey(v.ModifiedContainerDOI))
                        {
                            var properContainerDOICacheInfo = DOICacheInfoDict[v.ModifiedContainerDOI];
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

            /*

        var dummyDOIList = DummyDOIElementDict.Keys.ToList();
        dummyDOIList.ForEach((v) =>
        {
            if (doiElementDict.ContainsKey(v))
            {
                DummyDOIElementDict.Remove(v);
            }
        });
        */





            CommonFunctions.DecrementParagraphCounter();
            CommonFunctions.OutputSystemMessageFunction("Updating Modified Title UsingDBLP [END]");
        }

        public void ModifyType(string dataFolderPath)
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

            DOICacheInfoDict.Values.ToList().ForEach((v) =>
            {
                var doiElement = doiElementDict[v.DOI];
                if (v.ModifiedType.Length == 0)
                {
                    if (doiElement.Source == "CrossRef")
                    {
                        if (crossRefMapper.ContainsKey(doiElement.Type))
                        {
                            v.ModifiedType = crossRefMapper[doiElement.Type];
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
                if (DOICacheInfoDict.ContainsKey(v.DOI))
                {
                    var w = DOICacheInfoDict[v.DOI];

                    if (w.DOIRank == 0)
                    {
                        if (w.ModifiedContainerDOI.Length > 0 && !DOICacheInfoDict.ContainsKey(w.ModifiedContainerDOI))
                        {
                            DOICacheInfoDict[w.ModifiedContainerDOI] = new DOICacheInfo() { DOI = w.ModifiedContainerDOI, DOIRank = 1 };
                            this.DummyDOIElementDict[w.ModifiedContainerDOI] = new DOIElement() { DOI = w.ModifiedContainerDOI, Source = "Unknown", IsPrimary = false };
                        }


                        v.DOIReferences.ForEach((referenceDOI) =>
                        {
                            referenceDOI = doiAliasListMapper.ContainsKey(referenceDOI) ? doiAliasListMapper[referenceDOI] : referenceDOI;

                            if (!DOICacheInfoDict.ContainsKey(referenceDOI))
                            {
                                DOICacheInfoDict[referenceDOI] = new DOICacheInfo() { DOI = referenceDOI, DOIRank = 1 };
                                this.DummyDOIElementDict[referenceDOI] = new DOIElement() { DOI = referenceDOI, Source = "Unknown", IsPrimary = false };
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