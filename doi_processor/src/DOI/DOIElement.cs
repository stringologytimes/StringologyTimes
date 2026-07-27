using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using System.Text.Json;
using System;
using System.Globalization;

namespace DataProcessor
{
    public class DOIElement
    {
        public string DOI { get; set; } = "";
        public string Title { get; set; } = "";
        public List<AuthorInfo> Authors { get; set; } = new List<AuthorInfo>();

        public List<string> ISBNList { get; set; } = new List<string>();
        public List<string> ISSNList { get; set; } = new List<string>();
        public List<string> DOIAliasList { get; set; } = new List<string>();
        public string SeriesTitle { get; set; } = "";

        public string ContainerDOI { get; set; } = "";
        public string ContainerType { get; set; } = "";

        public string ContainerTitle { get; set; } = "";

        public string Type { get; set; } = "";
        public string Volume { get; set; } = "";
        public string Issue { get; set; } = "";

        public string Year { get; set; } = "";
        public string Month { get; set; } = "";

        public string Source { get; set; } = "";

        public string IdentifierTypeOrInstitution { get; set; } = "";


        public bool IsPrimary { get; set; } = false;

        public List<string> Tags { get; set; } = new List<string>();

        public List<string> DOIReferences { get; set; } = new List<string>();
        public List<string> UnknownReferences { get; set; } = new List<string>();



       

        /*
        private static int CountChar(string s, char c)
        {
            if (s is null) return 0;
            int count = 0;
            foreach (var ch in s)
                if (ch == c) count++;
            return count;
        }
        */
        /*
        public string ToJSONLine()
        {
            List<string> dataList = new List<string>();
            dataList.Add(JsonSerializer.Serialize(this.DOI));
            dataList.Add(JsonSerializer.Serialize(this.Type));
            dataList.Add(JsonSerializer.Serialize(this.Title));
            dataList.Add(JsonSerializer.Serialize(this.Year));
            dataList.Add(JsonSerializer.Serialize(this.Month));
            dataList.Add(JsonSerializer.Serialize(this.SeriesTitle));
            dataList.Add(JsonSerializer.Serialize(this.ContainerDOI));
            dataList.Add(JsonSerializer.Serialize(this.ContainerTitle));
            dataList.Add(JsonSerializer.Serialize(this.Volume));
            dataList.Add(JsonSerializer.Serialize(this.Source));
            dataList.Add(JsonSerializer.Serialize(this.IsPrimary));


            List<string> authorStringList = new List<string>();
            this.Authors.ForEach((v) =>
            {
                authorStringList.Add(v.to_JSON_Line());
            });
            var authorString = "[" + string.Join(",", authorStringList) + "]";
            dataList.Add(authorString);

            dataList.Add(JsonSerializer.Serialize(this.Tags.ToArray()));
            dataList.Add(JsonSerializer.Serialize(this.DOIReferences.ToArray()));
            dataList.Add(JsonSerializer.Serialize(this.UnknownReferences.ToArray()));
            dataList.Add(JsonSerializer.Serialize(this.ISBNList.ToArray()));
            dataList.Add(JsonSerializer.Serialize(this.ISSNList.ToArray()));
            string dataString = "[" + string.Join(",", dataList) + "]";
            return dataString;
        }
        */

        public string GetVolumeIssueString()
        {
            if(this.Volume.Length > 0 && this.Issue.Length > 0){
                return this.Volume + ":" + this.Issue;
            }
            else if(this.Volume.Length > 0){
                return this.Volume;
            }
            else if(this.Issue.Length > 0)
            {
                return "0" + ":" + this.Issue;
                //throw new Exception("Issue is not found");
            }
            else
            {
                return "";
            }
        }
        

        public void UpdateContainerDOI(DOICacheInfo v)
        {
            if (this.ContainerDOI.Length == 0 && v.ModifiedContainerDOI.Length > 0)
            {
                this.ContainerDOI = v.ModifiedContainerDOI;
            }

            
        }

        public static string GetDOIPrefix(string doi)
        {
            var prefix = doi.Split('/')[0];
            return prefix;
        }


        public static Dictionary<string, DOIElement> Load(string doiElementFilePath, bool checkFileExist)
        {
            CommonFunctions.OutputSystemMessageFunction("Loading from " + doiElementFilePath, ConsoleColor.Gray);
            var doiElementFileInfo = new FileInfo(doiElementFilePath);
            var doiDict = new Dictionary<string, DOIElement>();
            if (doiElementFileInfo.Exists)
            {

                var jsonLString = File.ReadAllText(doiElementFilePath);
                jsonLString.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList().ForEach((v) =>
                {
                    var doiElement = JsonSerializer.Deserialize<DOIElement>(v);
                    if (doiElement != null)
                    {
                        doiDict[doiElement.DOI] = doiElement;
                    }
                });
            }
            else
            {
                if (checkFileExist)
                {
                    throw new Exception("File not found: " + doiElementFilePath);
                }
            }
            return doiDict;
        }

        public static void Save(Dictionary<string, DOIElement> doiDict, string doiElementFilePath)
        {
            var copyList = doiDict.Values.ToList();
            copyList.Sort((a, b) => a.DOI.CompareTo(b.DOI));
            using (var writer = new StreamWriter(doiElementFilePath, false, Encoding.UTF8))
            {
                copyList.ForEach((v) =>
                {
                    string json = JsonSerializer.Serialize(v);
                    writer.WriteLine(json);
                });
            }
        }

    }
}