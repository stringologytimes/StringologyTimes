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
        public int Priority { get; set; } = 3;

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
        }
    }
}