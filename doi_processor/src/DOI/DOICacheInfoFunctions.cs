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
    class DOICacheInfoFunctions
    {
        
        public static void CreateProceedingsSeriesDummyDOIElement(IDictionary<string, DOIElement> dummyDOIElementDict, IDictionary<string, DOICacheInfo> doiCacheInfoDict, string proceedingsSeriesDummyDOI, DBLPProceedings proceedings, string minimum_year, string minimum_month)
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

        public static void CreateProceedingsDummyDOIElement(IDictionary<string, DOIElement> doiElementDict, IDictionary<string, DOIElement> dummyDOIElementDict, IDictionary<string, DOICacheInfo> doiCacheInfoDict, string proceedingsDOI, string proceedingsName, string containerDOI)
        {



            if (!dummyDOIElementDict.ContainsKey(proceedingsDOI) && !doiElementDict.ContainsKey(proceedingsDOI))
            {
                if (proceedingsDOI == "10.1007/11605126")
                {
                    CommonFunctions.OutputSystemMessageFunction("Proceedings DOI: " + proceedingsDOI, ConsoleColor.Red);
                    CommonFunctions.OutputSystemMessageFunction("Proceedings Name: " + proceedingsName, ConsoleColor.Red);
                    CommonFunctions.OutputSystemMessageFunction("Container DOI: " + containerDOI, ConsoleColor.Red);
                    Console.WriteLine(dummyDOIElementDict.ContainsKey(proceedingsDOI));
                    Console.WriteLine(doiElementDict.ContainsKey(proceedingsDOI));
                }

                var proceedingsDummyDOIElement = new DOIElement()
                {
                    DOI = proceedingsDOI,
                    Title = proceedingsName,
                    Source = "DUMMY",
                    IsPrimary = false,
                    Type = "Proceedings?",
                    ContainerDOI = containerDOI
                };

                dummyDOIElementDict[proceedingsDOI] = proceedingsDummyDOIElement;


            }

            doiCacheInfoDict[proceedingsDOI] = new DOICacheInfo()
            {
                DOI = proceedingsDOI,
                ModifiedTitle = proceedingsName,
                DOIRank = 1
            };


        }

        public static void CreateJournalDummyDOIElement(IDictionary<string, DOIElement> doiElementDict, IDictionary<string, DOIElement> dummyDOIElementDict, IDictionary<string, DOICacheInfo> doiCacheInfoDict, string journalDOI, string journalTitle)
        {


            if (!dummyDOIElementDict.ContainsKey(journalDOI) && !doiElementDict.ContainsKey(journalDOI))
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

                dummyDOIElementDict[journalDOI] = journalDummyDOIElement;
            }

            doiCacheInfoDict[journalDOI] = new DOICacheInfo()
            {
                DOI = journalDOI,
                ModifiedTitle = journalTitle,
                DOIRank = 1
            };

        }

        public static void CreatePreprintRepositoryDummyDOIElement(IDictionary<string, DOIElement> dummyDOIElementDict, IDictionary<string, DOICacheInfo> doiCacheInfoDict, string preprintRepositoryDOI, string preprintRepositoryTitle)
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


            if (!dummyDOIElementDict.ContainsKey(preprintRepositoryDOI))
            {
                dummyDOIElementDict[preprintRepositoryDOI] = preprintRepositoryDummyDOIElement;
            }

            doiCacheInfoDict[preprintRepositoryDOI] = new DOICacheInfo()
            {
                DOI = preprintRepositoryDOI,
                ModifiedTitle = preprintRepositoryTitle,
                DOIRank = 1
            };

        }

        public static void CreateJournalIssueDummyDOIElement(IDictionary<string, DOIElement> dummyDOIElementDict,
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


    }
}