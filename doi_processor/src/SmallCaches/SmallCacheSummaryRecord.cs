using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using System.Text.Json;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
namespace DataProcessor
{

    public class SmallCacheSummaryRecord
    {
        public string DOI { get; set; } = "";
        public string SourceCite { get; set; } = "";
        public string SourceStatus { get; set; } = "";

        public string CacheCreatedDate { get; set; } = "";
        public List<string> ISList { get; set; } = new List<string>();
        public int DOIRank { get; set; } = 3;

        public string ModifiedTitle { get; set; } = "";
        public string ModifiedType { get; set; } = "";
        public string ModifiedContainerDOI { get; set; } = "";
        public string ModifiedContainerDOIType { get; set; } = "";

        public static string ProceedingsArticleType = "Proceedings Article";
        public static string BookChapterType = "Book Chapter";
        public static string MonographChapterType = "Monograph Chapter";
        public static string ReferenceBookChapterType = "Reference Book Chapter";
        public static string ConferenceProceedingsType = "Conference Proceedings";
        public static string JournalArticleType = "Journal Article";
        public static string BookType = "Book";
        public static string JournalType = "Journal";
        public static string EditedBookType = "Edited Book";
        public static string ReferenceBookType = "Reference Book";
        public static string MonographType = "Monograph";
        public static string PostedContentType = "Posted Content";
        public static string PreprintType = "Preprint";
        public static string JournalIssueType = "Journal Issue";
        public static string ProceedingsCollecitonType = "Proceedings Collection";
        public static string PreprintRepositoryType = "Preprint Repository";


        public bool IsPreprint
        {
            get{return this.ModifiedType == SmallCacheSummaryRecord.PreprintType;}
        }

        public static string GetSmallCacheSummaryFilePath()
        {
            var smallCacheSummaryFilePath = Program.DataFolderPath + "/auto_generated/cache/small_cache_summary.jsonl";
            if (!File.Exists(smallCacheSummaryFilePath))
            {
                File.Create(smallCacheSummaryFilePath).Close();
            }
            return smallCacheSummaryFilePath;
        }
        public static string GetSmallCacheSummaryLogPath()
        {
            var smallCacheSummaryLogPath = Program.DataFolderPath + "/auto_generated/log/small_cache_summary.log";
            if (!File.Exists(smallCacheSummaryLogPath))
            {
                File.Create(smallCacheSummaryLogPath).Close();
            }
            return smallCacheSummaryLogPath;
        }

        public void UpdateForProceedings(string proceedingsNameWithYear, int proceedingsYear, string proceedingsCollectionDummyDOI, StreamWriter logFile)
        {
            this.ModifiedTitle = proceedingsNameWithYear;
            this.ModifiedContainerDOI = proceedingsCollectionDummyDOI;
            this.ModifiedType = SmallCacheSummaryRecord.ConferenceProceedingsType;
            this.ModifiedContainerDOIType = "DBLP";
            logFile.WriteLine($"Updated Proceedings: {this.DOI} -> {this.ModifiedTitle} -> {this.ModifiedContainerDOI} -> {this.ModifiedContainerDOIType} -> {this.ModifiedType}");
        }

        public void UpdateForProceedingsArticle(string proceedingsDOI, StreamWriter logFile)
        {
            this.ModifiedContainerDOI = proceedingsDOI;
            this.ModifiedContainerDOIType = "DBLP";
            this.ModifiedType = SmallCacheSummaryRecord.ProceedingsArticleType;
            logFile.WriteLine($"Updated Proceedings Article: {this.DOI} -> {this.ModifiedContainerDOI} -> {this.ModifiedContainerDOIType} -> {this.ModifiedType}");
        }



        public string ToJSONLine()
        {
            return JsonSerializer.Serialize(this);
        }

        public static Dictionary<string, SmallCacheSummaryRecord> Load(string doiCacheInfoFilePath)
        {
            var doiCacheInfoDict = new Dictionary<string, SmallCacheSummaryRecord>();
            var jsonLString = File.ReadAllText(doiCacheInfoFilePath);
            jsonLString.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList().ForEach((v) =>
            {
                var doiCacheInfo = JsonSerializer.Deserialize<SmallCacheSummaryRecord>(v);
                if (doiCacheInfo != null)
                {
                    doiCacheInfoDict[doiCacheInfo.DOI] = doiCacheInfo;
                }
            });
            return doiCacheInfoDict;
        }

        public static void Save(Dictionary<string, SmallCacheSummaryRecord> doiCacheInfoDict, string doiCacheInfoFilePath)
        {
            var copyList = doiCacheInfoDict.Values.ToList();
            copyList.Sort((a, b) => a.DOI.CompareTo(b.DOI));
            using (var writer = new StreamWriter(doiCacheInfoFilePath, false, Encoding.UTF8))
            {
                copyList.ForEach((v) =>
                {
                    writer.WriteLine(v.ToJSONLine());
                });
            }
        }

        public static bool TitleParentCheck(DOIElement child, DOIElement parent)
        {
            if(child.DOI == parent.DOI)
            {
                return false;
            }
            if(child.Type == parent.Type)
            {
                return false;
            }
            if(child.Year != parent.Year)
            {
                return false;
            }

            var childDOIPrefix = DOIElement.GetDOIPrefix(child.DOI);
            var parentDOIPrefix = DOIElement.GetDOIPrefix(parent.DOI);
            if(childDOIPrefix != parentDOIPrefix)
            {
                return false;
            }
            return true;
            
        }

        public void UpdateContainerDOI(Dictionary<string, DOIElement> doiElementDict, Dictionary<string, string> isbnDictionary, Dictionary<string, string> issnDictionary, Dictionary<string, List<string>> titleDictionary, StreamWriter logFile)
        {
            var doiElement = doiElementDict[this.DOI];
            if (this.ModifiedContainerDOI.Length == 0)
            {
                if (doiElement.ContainerDOI.Length > 0 && this.DOI != doiElement.ContainerDOI)
                {
                    this.ModifiedContainerDOI = doiElement.ContainerDOI;
                    this.ModifiedContainerDOIType = doiElement.ContainerType;
                }
            }


            if (this.ModifiedContainerDOI.Length == 0)
            {
                for (int i = 0; i < doiElement.ISBNList.Count; i++)
                {
                    var ISBN = doiElement.ISBNList[i];
                    if (isbnDictionary.ContainsKey(ISBN) && ISBN.Length > 0 && this.DOI != isbnDictionary[ISBN])
                    {
                        this.ModifiedContainerDOI = isbnDictionary[ISBN];
                        this.ModifiedContainerDOIType = "ISBN";
                        break;
                    }
                }
            }

            var title = doiElement.ContainerTitle;
            if (this.ModifiedContainerDOI.Length == 0 && title.Length > 0 && titleDictionary.ContainsKey(title))
            {
                var containerDOICandidateList = titleDictionary[title];
                var candidateList = new List<string>();
                foreach (var containerDOI in containerDOICandidateList)
                {
                    if (doiElementDict.ContainsKey(containerDOI) && TitleParentCheck(doiElement, doiElementDict[containerDOI]))
                    {
                        candidateList.Add(containerDOI);
                    }
                }

                if (candidateList.Count == 1)
                {
                    this.ModifiedContainerDOI = candidateList[0];
                    this.ModifiedContainerDOIType = "Title";
                }
                else if (candidateList.Count > 1)
                {
                    Console.WriteLine("Multiple container DOI candidates found for title: " + title);
                    throw new Exception("Multiple container DOI candidates found for title: " + title);
                }


            }

        }

        public void UpdateSourceCite(HashSet<string> crossRefDOIPrefixSet, HashSet<string> dataCiteDOIPrefixSet)
        {
            var doiPrefix = DOIFunctions.GetPrefix(this.DOI);
            if (doiPrefix == "dummy")
            {
                this.SourceCite = "DUMMY";
                this.SourceStatus = "Custom";
            }
            else if (crossRefDOIPrefixSet.Contains(doiPrefix))
            {
                this.SourceCite = "CrossRef";
            }
            else if (dataCiteDOIPrefixSet.Contains(doiPrefix))
            {
                this.SourceCite = "DataCite";
            }
            else
            {
                this.SourceCite = "Unknown";
                this.SourceStatus = "Unknown";
            }
        }



        public static Dictionary<string, DOIElement> BuildDOIElementDictionary(string dataFolderPath, IDictionary<string, SmallCacheSummaryRecord> doiCacheInfoDict)
        {
            var crossRefFoundDOIFilePath = dataFolderPath + "/auto_generated/cache/crossref_cache/small_cache/found_doi.jsonl";
            var crossRefFoundExternalDOIFilePath = dataFolderPath + "/auto_generated/cache/crossref_cache/small_cache/found_external_doi.jsonl";


            var crossRefFoundDOI = CrossRefJSONLLoader.LoadFoundDOI(crossRefFoundDOIFilePath);
            var crossRefFoundExternalDOI = CrossRefJSONLLoader.LoadFoundExternalDOI(crossRefFoundExternalDOIFilePath);

            var dataCiteFoundDOI = DataCiteLocalCache.Load(dataFolderPath);
            var dataCiteFoundExternalDOI = DataCiteExternalFoundDOICache.Load(dataFolderPath);

            var dummyDOIElementDict = DOIElement.Load(DummyCacheManager.GetDummyCacheFilePath(dataFolderPath), false);

            var doiAliasMapper = DOIElementPreprocessor.LoadDOIAliasListMapper(dataFolderPath);

            Console.WriteLine("CrossRef Found DOI: " + crossRefFoundDOI.Count);
            Console.WriteLine("CrossRef Found External DOI: " + crossRefFoundExternalDOI.Count);
            Console.WriteLine("DataCite Found DOI: " + dataCiteFoundDOI.Count);
            Console.WriteLine("DataCite Found External DOI: " + dataCiteFoundExternalDOI.Count);

            var doiElementDict = new Dictionary<string, DOIElement>();

            foreach (var v in doiCacheInfoDict)
            {
                var doiElement = new DOIElement();
                bool b = false;
                if (crossRefFoundDOI.ContainsKey(v.Key))
                {
                    doiElement = CrossRefParser.Parse(crossRefFoundDOI[v.Key]);
                    b = true;
                }
                else if (crossRefFoundExternalDOI.ContainsKey(v.Key))
                {
                    doiElement = CrossRefParser.Parse(crossRefFoundExternalDOI[v.Key]);
                    b = true;
                }
                else if (dataCiteFoundDOI.ContainsKey(v.Key))
                {
                    doiElement = DataCiteParser.Parse(dataCiteFoundDOI[v.Key]);
                    b = true;
                }
                else if (dataCiteFoundExternalDOI.ContainsKey(v.Key))
                {
                    doiElement = DataCiteParser.Parse(dataCiteFoundExternalDOI[v.Key]);
                    b = true;
                }
                else if (v.Value.SourceCite == "DUMMY")
                {
                    doiElement = dummyDOIElementDict[v.Key];
                    b = true;
                }


                if (b)
                {
                    doiElementDict[v.Key] = doiElement;
                    doiElement.IsPrimary = v.Value.DOIRank == 0;

                    if (v.Value.ModifiedContainerDOI.Length > 0)
                    {
                        doiElement.ContainerDOI = v.Value.ModifiedContainerDOI;
                        doiElement.ContainerType = v.Value.ModifiedContainerDOIType;
                    }
                    if (v.Value.ModifiedTitle.Length > 0)
                    {
                        doiElement.Title = v.Value.ModifiedTitle;
                    }
                    if (v.Value.ModifiedType.Length > 0)
                    {
                        doiElement.Type = v.Value.ModifiedType;
                    }

                    for (int i = 0; i < doiElement.DOIReferences.Count; i++)
                    {
                        var doiReference = doiElement.DOIReferences[i];
                        if (doiAliasMapper.ContainsKey(doiReference))
                        {
                            doiElement.DOIReferences[i] = doiAliasMapper[doiReference];
                        }
                    }

                    if (!doiElement.IsPrimary)
                    {
                        doiElement.DOIReferences.Clear();
                    }
                }
            }



            doiElementDict.ToList().ForEach((v) =>
            {
                if (v.Value.ContainerDOI.Length > 0 && doiElementDict.ContainsKey(v.Value.ContainerDOI))
                {
                    var containerDOIElement = doiElementDict[v.Value.ContainerDOI];
                    v.Value.ContainerTitle = containerDOIElement.Title;
                    
                }

                
            });



            return doiElementDict;
        }

    }
}