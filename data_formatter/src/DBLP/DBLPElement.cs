using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using System.Text.Json;

namespace DataProcessor
{
    class DBLPElement
    {
        public List<string> Authors { get; set; } = new List<string>();
        public string Title { get; set; } = "";
        public int? Year { get; set; } = null;
        public int? Month { get; set; } = null;
        //public string Journal { get; set; } = "";
        public string DOI { get; set; } = "";
        //public string Url { get; set; } = "";
        public string Volume { get; set; } = "";
        public string PaperType { get; set; } = "";
        public string BookTitleOrJournal { get; set; } = "";
        public List<string> Tags { get; set; } = new List<string>();
        public string ToJSONLine()
        {
            List<string> dataList = new List<string>();
            dataList.Add(JsonSerializer.Serialize(this.DOI));
            dataList.Add(JsonSerializer.Serialize(this.PaperType));
            dataList.Add(JsonSerializer.Serialize(this.BookTitleOrJournal));
            dataList.Add(JsonSerializer.Serialize(this.Title));
            dataList.Add(this.Year?.ToString() ?? "");
            dataList.Add(this.Month?.ToString() ?? "");

            string authorString = JsonSerializer.Serialize(this.Authors);
            dataList.Add(authorString);
            //dataList.Add(JsonSerializer.Serialize(this.Journal));
            //dataList.Add(JsonSerializer.Serialize(this.Url));
            dataList.Add(JsonSerializer.Serialize(this.Volume));
            dataList.Add(JsonSerializer.Serialize(this.Tags));

            string dataString = "[" + string.Join(",", dataList) + "]";
            return dataString;
        }
        public static string? getDOI(string url)
        {
            if (url.IndexOf("https://doi.org/") == 0)
            {
                return url.Substring("https://doi.org/".Length);
            }
            else if (url.IndexOf("http://doi.org/") == 0)
            {
                return url.Substring("http://doi.org/".Length);
            }
            else if (url.IndexOf("https://arxiv.org/abs/") == 0)
            {
                var psuf = url.Substring("https://arxiv.org/abs/".Length);
                return "10.48550/arXiv." + psuf;
            }
            else if (url.IndexOf("http://arxiv.org/abs/") == 0)
            {
                var psuf = url.Substring("http://arxiv.org/abs/".Length);
                return "10.48550/arXiv." + psuf;
            }
            else
            {
                return null;
            }
        }
        public static DBLPElement BuildFromXML(XElement x, List<string> tags)
        {
            var dblpElement = new DBLPElement();

            dblpElement.Tags = tags.ToList();


            var paperType = x.Name.ToString();
            if (paperType == "article")
            {
                dblpElement.PaperType = "article";
                dblpElement.BookTitleOrJournal = x.Element("journal")?.Value ?? "";
            }
            else if (paperType == "inproceedings")
            {
                dblpElement.PaperType = "inproceedings";
                dblpElement.BookTitleOrJournal = x.Element("booktitle")?.Value ?? "";
            }
            else if (paperType == "proceedings")
            {
                dblpElement.PaperType = "proceedings";
                dblpElement.BookTitleOrJournal = x.Element("booktitle")?.Value ?? "";
            }
            else if (paperType == "incollection")
            {
                dblpElement.PaperType = "incollection";
                dblpElement.BookTitleOrJournal = x.Element("booktitle")?.Value ?? "";
            }
            else
            {
                dblpElement.PaperType = "unknown";
            }

            var authors = x.Elements("author");
            if (authors != null)
            {
                foreach (var author in authors)
                {
                    dblpElement.Authors.Add(author.Value);
                }
            }
            var title = x.Element("title");
            if (title != null)
            {
                dblpElement.Title = title.Value;
            }
            var year = x.Element("year");
            if (year != null)
            {
                dblpElement.Year = int.Parse(year.Value);
            }
            /*
            var journal = x.Element("journal");
            if (journal != null)
            {
                dblpElement.Journal = journal.Value;
            }
            */
            var ee = x.Element("ee");
            if (ee != null)
            {
                var eeURL = ee.FirstNode?.ToString() ?? "";
                var doi = DBLPElement.getDOI(eeURL);
                if (doi != null)
                {
                    dblpElement.DOI = doi.ToLower();
                }
            }
            /*
            var url = x.Element("url");
            if (url != null)
            {
                dblpElement.Url = url.Value;
            }
            */
            var volume = x.Element("volume");
            if (volume != null)
            {
                dblpElement.Volume = volume.Value;
            }
            return dblpElement;
        }

        public static DBLPElement BuildFromXML(XElement x)
        {
            var tags = new List<string>();
            return BuildFromXML(x, tags);
        }
    }
}

