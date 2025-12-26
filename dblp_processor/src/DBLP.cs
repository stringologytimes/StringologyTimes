using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using System.Text.Json;

namespace DBLPProcessor
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
        public string to_JSON_Line()
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
        public static DBLPElement from_XML(XElement x, List<string> tags)
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
                    dblpElement.DOI = doi;
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
    }
    class Processor
    {
        public static List<DBLPElement> Process(string xmlPath, Dictionary<string, List<string>> doiToTagMapper)
        {

            //HashSet<string> doiHashSet = new HashSet<string>();
            HashSet<string> journalURLHashSet = new HashSet<string>();
            HashSet<string> ProceedingNameHashSet = new HashSet<string>();

            /*
            StreamReader sr = new StreamReader(urlListPath, System.Text.Encoding.UTF8);
            var urlText = sr.ReadToEnd().Replace("\r\n", "\n");
            var urlLines = urlText.Split("\n");
            foreach (var line in urlLines)
            {
                if (line.IndexOf("^JournalURL") == 0)
                {
                    var words = line.Split(",");
                    journalURLHashSet.Add(words[1]);
                }
                else if (line.IndexOf("^ProceedingName") == 0)
                {
                    var words = line.Split(",");
                    ProceedingNameHashSet.Add(words[1]);
                }
                else if (line.IndexOf("10.") == 0)
                {
                    //var url = "https://doi.org/" + line;
                    //Console.WriteLine(url + " / " + line);
                    doiHashSet.Add(line);
                }
                else if (line.Length > 3)
                {
                }
            }
            */

            List<DBLPElement> dblpElements = new List<DBLPElement>();

            var stream = DBLPProcessor.DBLPProcessorFunctions.StreamCustomerItem(xmlPath);
            var counter = 0;
            XElement root = new XElement("dblp");

            foreach (var v in stream)
            {
                if (counter % 100000 == 0)
                {
                    Console.WriteLine(counter);
                }
                var booktitleElement = v.Element("booktitle");
                var b1 = v.Name == "inproceedings" && booktitleElement != null && ProceedingNameHashSet.Contains(booktitleElement.Value);
                var urlNode = v.Element("url");
                var b2 = false;
                if (urlNode != null)
                {
                    var url = "https://dblp.org/" + urlNode.Value.Split("#")[0];
                    if (journalURLHashSet.Contains(url))
                    {
                        b2 = true;
                    }
                }
                var eeChildren = v.Elements("ee");
                foreach (var eeChild in eeChildren)
                {
                    if (eeChild != null)
                    {
                        var url = eeChild.FirstNode?.ToString() ?? "";
                        var doi = DBLPElement.getDOI(url);

                        if (doi != null && doiToTagMapper.ContainsKey(doi))
                        {
                            root.Add(v);

                            var dblpElement = new DBLPElement();
                            dblpElement = DBLPElement.from_XML(v, doiToTagMapper[doi]);
                            dblpElements.Add(dblpElement);


                            //doiToTagMapper.Remove(doi);
                            Console.WriteLine(doi);
                            break;
                        }
                        if (b1 || b2)
                        {
                            root.Add(v);

                            var dblpElement = new DBLPElement();
                            var tags = new List<string>();
                            if (doi != null && doiToTagMapper.ContainsKey(doi))
                            {
                                tags = doiToTagMapper[doi];
                            }
                            else
                            {
                                tags = new List<string>();
                            }

                            dblpElement = DBLPElement.from_XML(v, tags);
                            dblpElements.Add(dblpElement);


                            Console.WriteLine(doi);
                            break;
                        }

                    }
                }
                counter++;
            }
            return dblpElements;


            /*
            foreach (var doi in doiToTagMapper.Keys)
            {
                Console.WriteLine("Not found: " + doi);
            }
            */


        }
    }
}

