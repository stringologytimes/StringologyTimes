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
    class SmallCacheSummaryFunctions
    {

        public static void CreateProceedingsSeriesDummyDOIElement(string proceedingsSeriesDummyDOI, DBLPProceedings proceedings, string minimum_year, string minimum_month, SmallCacheManager manager)
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


            if (!manager.DummyDOIElementDict.ContainsKey(proceedingsSeriesDummyDOI))
            {
                manager.DummyDOIElementDict[proceedingsSeriesDummyDOI] = proceedingsSeriesDummyDOIElement;
            }

            manager.SmallCacheSummaryRecordDict[proceedingsSeriesDummyDOI] = new SmallCacheSummaryRecord()
            {
                DOI = proceedingsSeriesDummyDOI,
                ModifiedTitle = proceedings.SeriesTitle,
                DOIRank = 1
            };
            manager.SmallCacheSummaryLogFile.WriteLine($"Added Proceedings Series Dummy DOI in DOICacheInfoFunctions: {proceedingsSeriesDummyDOI}");

        }

        public static void CreateProceedingsDummyDOIElement(string proceedingsDOI, string proceedingsName, string containerDOI, IDictionary<string, DOIElement> doiElementDict, SmallCacheManager manager)
        {
            if (!manager.DummyDOIElementDict.ContainsKey(proceedingsDOI) && !doiElementDict.ContainsKey(proceedingsDOI))
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

                manager.DummyDOIElementDict[proceedingsDOI] = proceedingsDummyDOIElement;


            }

            manager.SmallCacheSummaryRecordDict[proceedingsDOI] = new SmallCacheSummaryRecord()
            {
                DOI = proceedingsDOI,
                ModifiedTitle = proceedingsName,
                DOIRank = 1
            };
            manager.SmallCacheSummaryLogFile.WriteLine($"Added Proceedings Dummy DOI in DOICacheInfoFunctions: {proceedingsDOI}");


        }

        public static void CreateJournalDummyDOIElement(string journalDOI, string journalTitle, IDictionary<string, DOIElement> doiElementDict, SmallCacheManager manager)
        {


            if (!manager.DummyDOIElementDict.ContainsKey(journalDOI) && !doiElementDict.ContainsKey(journalDOI))
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

                manager.DummyDOIElementDict[journalDOI] = journalDummyDOIElement;
            }

            manager.SmallCacheSummaryRecordDict[journalDOI] = new SmallCacheSummaryRecord()
            {
                DOI = journalDOI,
                ModifiedTitle = journalTitle,
                DOIRank = 1
            };
            manager.SmallCacheSummaryLogFile.WriteLine($"Added Journal Dummy DOI in DOICacheInfoFunctions: {journalDOI}");
        }

        public static void CreatePreprintRepositoryDummyDOIElement(string preprintRepositoryDOI, string preprintRepositoryTitle, SmallCacheManager manager)
        {
            var preprintRepositoryDummyDOIElement = new DOIElement()
            {
                DOI = preprintRepositoryDOI,
                Title = preprintRepositoryTitle,
                Source = "DUMMY",
                IsPrimary = false,
                Type = "PreprintRepository",
                ContainerDOI = ""
            };


            if (!manager.DummyDOIElementDict.ContainsKey(preprintRepositoryDOI))
            {
                manager.DummyDOIElementDict[preprintRepositoryDOI] = preprintRepositoryDummyDOIElement;
            }

            manager.SmallCacheSummaryRecordDict[preprintRepositoryDOI] = new SmallCacheSummaryRecord()
            {
                DOI = preprintRepositoryDOI,
                ModifiedTitle = preprintRepositoryTitle,
                DOIRank = 1
            };
            manager.SmallCacheSummaryLogFile.WriteLine($"Added Preprint Repository Dummy DOI in DOICacheInfoFunctions: {preprintRepositoryDOI}");
        }

        public static void CreateJournalIssueDummyDOIElement(string journalIssueDummyDOI, string journalIssueTitle, string containerDOI, string year, string month, SmallCacheManager manager)
        {
            var journalIssueDummyDOIElement = new DOIElement()
            {
                DOI = journalIssueDummyDOI,
                Title = journalIssueTitle,
                Source = "DUMMY",
                IsPrimary = false,
                Type = "Journal-Issue",
                ContainerDOI = containerDOI,
                Year = year.ToString(),
                Month = month.ToString()
            };

            if (!manager.DummyDOIElementDict.ContainsKey(journalIssueDummyDOI))
            {
                manager.DummyDOIElementDict[journalIssueDummyDOI] = journalIssueDummyDOIElement;
            }

            manager.SmallCacheSummaryRecordDict[journalIssueDummyDOI] = new SmallCacheSummaryRecord()
            {
                DOI = journalIssueDummyDOI,
                ModifiedTitle = journalIssueTitle,
                DOIRank = 1
            };
            manager.SmallCacheSummaryLogFile.WriteLine($"Added Journal Issue Dummy DOI in DOICacheInfoFunctions: {journalIssueDummyDOI}");
        }

        public static int ComputeProceedingsYear(int? proceedingsYear, string doiYear)
        {

            if (proceedingsYear != null)
            {
                if (doiYear.Length > 0)
                {

                    var doiYearInt = int.Parse(doiYear);
                    if (proceedingsYear.Value > doiYearInt)
                    {
                        return doiYearInt;
                    }
                    else
                    {
                        return proceedingsYear.Value;
                    }

                }
                else
                {
                    return proceedingsYear.Value;
                }

            }
            else if (doiYear.Length > 0)
            {
                return int.Parse(doiYear);
            }
            else
            {
                return 0;
            }


        }

        public static void UpdateProcessForProceedingsArticle(DOIElement doiElement, Dictionary<string, DOIElement> doiElementDict, SmallCacheManager manager, DBLPProceedingsSeriesDictionary dblpSeriesDictionary)
        {
            var seriesTitleAndKey = dblpSeriesDictionary.SearchSeriesTitleAndKeyByDOI(doiElement.DOI);
            var record = manager.SmallCacheSummaryRecordDict[doiElement.DOI];

            if (seriesTitleAndKey != null)
            {
                if (!dblpSeriesDictionary.Series.ContainsKey(seriesTitleAndKey.Value.Key))
                {
                    throw new Exception("Series Title and Key: " + seriesTitleAndKey.Value.Key + " is not found in dblpSeriesDictionary.Series");
                }


                var proceedingsSeries = dblpSeriesDictionary.Series[seriesTitleAndKey.Value.Key];
                var proceedings = proceedingsSeries.GetProceedings(seriesTitleAndKey.Value.Value);
                var proceedingsSeriesTitle = proceedingsSeries.SeriesTitle;
                var proceedingsYear = SmallCacheSummaryFunctions.ComputeProceedingsYear(proceedings.Year, doiElement.Year);
                var proceedingsName = proceedings.SeriesTitle;

                var proceedingsDOI = doiElement.ContainerDOI;
                if (proceedingsDOI.Length == 0)
                {
                    proceedingsDOI = proceedings.DOI.Length > 0 ? proceedings.DOI : DOIFunctions.CreateDummyDOI("proceedings", proceedingsName);
                }

                var (minimum_year, minimum_month) = proceedingsSeries.GetMinimumYearAndMonth();


                var proceedingsSeriesDummyDOI = DOIFunctions.CreateDummyDOI("proceedings_series", proceedingsSeriesTitle);
                if (!manager.SmallCacheSummaryRecordDict.ContainsKey(proceedingsSeriesDummyDOI) && record.DOIRank == 0)
                {
                    SmallCacheSummaryFunctions.CreateProceedingsSeriesDummyDOIElement(proceedingsSeriesDummyDOI, proceedings, minimum_year.ToString(), minimum_month.ToString(), manager);
                }

                if (!manager.SmallCacheSummaryRecordDict.ContainsKey(proceedingsDOI) && record.DOIRank == 0)
                {
                    SmallCacheSummaryFunctions.CreateProceedingsDummyDOIElement(proceedingsDOI, proceedingsName, proceedingsSeriesDummyDOI, doiElementDict, manager);
                }

                if (manager.SmallCacheSummaryRecordDict.ContainsKey(proceedingsDOI))
                {
                    var proceedingsCache = manager.SmallCacheSummaryRecordDict[proceedingsDOI];
                    proceedingsCache.UpdateForProceedings(proceedingsName, proceedingsYear, proceedingsSeriesDummyDOI, manager.SmallCacheSummaryLogFile);
                }

                if (record.ModifiedContainerDOI.Length == 0)
                {
                    record.UpdateForProceedingsArticle(proceedingsDOI, manager.SmallCacheSummaryLogFile);
                }


                //proceedingsSeries.
            }


        }

        public static void UpdateForPreprint(DOIElement doiElement, SmallCacheManager manager)
        {
            var record = manager.SmallCacheSummaryRecordDict[doiElement.DOI];
            if (!record.IsPreprint && doiElement.IdentifierTypeOrInstitution.Length > 0)
            {
                var preprintRepositoryDOI = DOIFunctions.CreateDummyDOI("preprint_repository", doiElement.IdentifierTypeOrInstitution);
                if (!manager.SmallCacheSummaryRecordDict.ContainsKey(preprintRepositoryDOI) && record.DOIRank == 0)
                {
                    SmallCacheSummaryFunctions.CreatePreprintRepositoryDummyDOIElement(preprintRepositoryDOI, doiElement.IdentifierTypeOrInstitution, manager);
                }

                record.ModifiedContainerDOI = preprintRepositoryDOI;
                record.ModifiedContainerDOIType = "Metadata";
                record.ModifiedType = "Preprint";
            }
        }

        public static void UpdateForPostedContent(DOIElement doiElement, SmallCacheManager manager)
        {
            var record = manager.SmallCacheSummaryRecordDict[doiElement.DOI];
            if (!record.IsPreprint && doiElement.IdentifierTypeOrInstitution == "bioRxiv")
            {
                record.ModifiedType = "Preprint";

                var bioRxivDOI = DOIFunctions.CreateDummyDOI("preprint_repository", "biorxiv");
                if (!manager.SmallCacheSummaryRecordDict.ContainsKey(bioRxivDOI) && record.DOIRank == 0)
                {
                    SmallCacheSummaryFunctions.CreatePreprintRepositoryDummyDOIElement(bioRxivDOI, "bioRxiv", manager);
                }

                record.ModifiedContainerDOI = bioRxivDOI;
                record.ModifiedContainerDOIType = "Metadata";

            }

        }

        public static void UpdateForJournalArticle(DOIElement doiElement, IDictionary<string, DOIElement> doiElementDict, SmallCacheManager manager)
        {
            var record = manager.SmallCacheSummaryRecordDict[doiElement.DOI];
            var journalTitle = doiElement.ContainerTitle;
            var volumeIssueString = doiElement.GetVolumeIssueString();
            var journalDOI = doiElement.ContainerDOI;
            if (journalDOI.Length == 0)
            {
                journalDOI = DOIFunctions.CreateDummyDOI("journal", journalTitle);
            }

            if (!manager.SmallCacheSummaryRecordDict.ContainsKey(journalDOI) && record.DOIRank == 0)
            {
                SmallCacheSummaryFunctions.CreateJournalDummyDOIElement(journalDOI, journalTitle, doiElementDict, manager);
            }

            var journalIssueTitle = journalTitle + "(" + volumeIssueString + ")";
            var journalIssueDummyDOI = DOIFunctions.CreateDummyDOI("journal_issue", journalIssueTitle);
            var year = doiElement.Year;
            var month = doiElement.Month;

            if (!manager.SmallCacheSummaryRecordDict.ContainsKey(journalIssueDummyDOI) && record.DOIRank == 0)
            {
                SmallCacheSummaryFunctions.CreateJournalIssueDummyDOIElement(journalIssueDummyDOI, journalIssueTitle, journalDOI, year, month, manager);
            }

            if (record.ModifiedContainerDOI.Length == 0)
            {
                record.ModifiedContainerDOI = journalIssueDummyDOI;
                record.ModifiedContainerDOIType = "Metadata";
                record.ModifiedType = "Journal-Article";
            }
        }
    }
}