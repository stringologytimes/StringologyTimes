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

        public bool IsJournalArticle
        {
            get{return this.Type == "journal-article";}
        }
        public bool IsPostedContent
        {
            get{return this.Type == "posted-content";}
        }
        public bool IsPreprint
        {
            get{return this.Type == "Preprint";}
        }
        public bool IsProceedingsArticle
        {
            get{return this.Type == "proceedings-article" || this.Type == "ConferencePaper";}
        }
        public bool IsBookChapter
        {
            get{return this.Type == "book-chapter";}
        }
        public bool IsBook
        {
            get{return this.Type == "book" || this.Type == "Book";}
        }
        public bool IsReferenceBook
        {
            get{return this.Type == "reference-book" || this.Type == "ReferenceBook";}
        }
        public bool IsMonograph
        {
            get{return this.Type == "monograph" || this.Type == "Monograph";}
        }


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
        

        public void UpdateContainerDOI(SmallCacheSummaryRecord v)
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