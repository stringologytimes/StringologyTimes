using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using System.Text.Json;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
namespace DataProcessor
{

    public class DOICacheInfo
    {
        public string DOI { get; set; } = "";
        public string SourceCite { get; set; } = "";
        public string SourceStatus { get; set; } = "";

        public string Date { get; set; } = "";
        public string ContainerDOI { get; set; } = "";
        public int DOIRank { get; set; } = 3;

        public string ToJSONLine()
        {
            return JsonSerializer.Serialize(this);
            /*
                        List<string> dataList = new List<string>();
                        dataList.Add(JsonSerializer.Serialize(this.DOI));
                        dataList.Add(JsonSerializer.Serialize(this.Priority));
                        dataList.Add(JsonSerializer.Serialize(this.SourceCite));
                        dataList.Add(JsonSerializer.Serialize(this.SourceStatus));
                        dataList.Add(JsonSerializer.Serialize(this.ContainerDOI));
                        dataList.Add(JsonSerializer.Serialize(this.Date));

                        string dataString = "[" + string.Join(",", dataList) + "]";
                        return dataString;
                        */
        }

        public static Dictionary<string, DOICacheInfo> Load(string doiCacheInfoFilePath)
        {
            var doiCacheInfoDict = new Dictionary<string, DOICacheInfo>();
            var jsonLString = File.ReadAllText(doiCacheInfoFilePath);
            jsonLString.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList().ForEach((v) =>
            {
                var doiCacheInfo = JsonSerializer.Deserialize<DOICacheInfo>(v);
                if (doiCacheInfo != null)
                {
                    doiCacheInfoDict[doiCacheInfo.DOI] = doiCacheInfo;
                }
            });
            return doiCacheInfoDict;
        }

        public static void Save(Dictionary<string, DOICacheInfo> doiCacheInfoDict, string doiCacheInfoFilePath)
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

        public void UpdateContainerDOI(DOIElement doiElement, Dictionary<string, string> isbnDictionary, Dictionary<string, string> titleDictionary, StreamWriter logFile)
        {
            if (this.ContainerDOI.Length == 0)
            {
                if (doiElement.ContainerDOI.Length > 0 && this.DOI != doiElement.ContainerDOI)
                {
                    this.ContainerDOI = doiElement.ContainerDOI;
                }
            }


            if (this.ContainerDOI.Length == 0)
            {
                for (int i = 0; i < doiElement.ISBNList.Count; i++)
                {
                    var ISBN = doiElement.ISBNList[i];
                    if (isbnDictionary.ContainsKey(ISBN) && ISBN.Length > 0 && this.DOI != isbnDictionary[ISBN])
                    {
                        this.ContainerDOI = isbnDictionary[ISBN];
                        break;
                    }
                }
            }
            if (this.ContainerDOI.Length == 0 && doiElement.ContainerTitle.Length > 0)
            {
                if (titleDictionary.ContainsKey(doiElement.ContainerTitle) && this.DOI != titleDictionary[doiElement.ContainerTitle])
                {
                    this.ContainerDOI = titleDictionary[doiElement.ContainerTitle];
                    logFile.WriteLine("Matched Container Title: " + this.DOI + " -> " + this.ContainerDOI);
                }
            }

            //if(doiElement.ISBNList)
        }

        public void UpdateSourceCite(HashSet<string> crossRefDOIPrefixSet, HashSet<string> dataCiteDOIPrefixSet)
        {
            var doiPrefix = DOIFunctions.GetPrefix(this.DOI);
            if (doiPrefix == "99.9999")
            {
                this.SourceCite = "DUMMY-ISSN";
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

        

        public static Dictionary<string, DOIElement> BuildDOIElementDictionary(string dataFolderPath, IDictionary<string, DOICacheInfo> doiCacheInfoDict)
        {
            var crossRefFoundDOIFilePath = dataFolderPath + "/auto_generated/cache/crossref_cache/small_cache/found_doi.jsonl";
            var crossRefFoundExternalDOIFilePath = dataFolderPath + "/auto_generated/cache/crossref_cache/small_cache/found_external_doi.jsonl";


            var crossRefFoundDOI = CrossRefJSONLLoader.LoadFoundDOI(crossRefFoundDOIFilePath);
            var crossRefFoundExternalDOI = CrossRefJSONLLoader.LoadFoundExternalDOI(crossRefFoundExternalDOIFilePath);

            var dataCiteFoundDOI = DataCiteLocalCache.Load(dataFolderPath);
            var dataCiteFoundExternalDOI = DataCiteExternalFoundDOICache.Load(dataFolderPath);

            Console.WriteLine("CrossRef Found DOI: " + crossRefFoundDOI.Count);
            Console.WriteLine("CrossRef Found External DOI: " + crossRefFoundExternalDOI.Count);
            Console.WriteLine("DataCite Found DOI: " + dataCiteFoundDOI.Count);
            Console.WriteLine("DataCite Found External DOI: " + dataCiteFoundExternalDOI.Count);

            var doiElementDict = new Dictionary<string, DOIElement>();
            doiCacheInfoDict.ToList().ForEach((v) =>
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


                if (b)
                {
                    doiElement.IsPrimary = v.Value.DOIRank == 0;
                    doiElementDict[v.Key] = doiElement;                    
                }

            });

            /*
            var logFilePath = dataFolderPath + "/auto_generated/log/build_doi_element_dictionary.log";
            var logFile = new StreamWriter(logFilePath, true);
            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : Start");


            var doiElementDict = new Dictionary<string, DOIElement>();
            var doiElementCachePath = DOIElementPreprocessor.GetCachePath(dataFolderPath);
            var doiElementCache = DOIElement.Load(doiElementCachePath, false);
            doiCacheInfoDict.ToList().ForEach((v) =>
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
            */
            return doiElementDict;
        }
        
    }
}