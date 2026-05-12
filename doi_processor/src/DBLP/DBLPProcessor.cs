using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DataProcessor
{
    class DBLPProcessor
    {
        public static List<DBLPElement> Process(string xmlPath, Dictionary<string, List<string>> doiToTagMapper)
        {

            //HashSet<string> doiHashSet = new HashSet<string>();
            HashSet<string> journalURLHashSet = new HashSet<string>();
            HashSet<string> ProceedingNameHashSet = new HashSet<string>();

            List<DBLPElement> dblpElements = new List<DBLPElement>();

            var stream = DataProcessor.DBLPProcessorFunctions.StreamCustomerItem(xmlPath);
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
                            dblpElement = DBLPElement.BuildFromXML(v, doiToTagMapper[doi]);
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

                            dblpElement = DBLPElement.BuildFromXML(v, tags);
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
        public static DBLPProceedingsSeriesDictionary CollectProceedings(string xmlPath, string replaceTSVPath)
        {
            var doiDictionary = new Dictionary<string, List<string>>();
            var proceedingsSeriesDictionary = new DBLPProceedingsSeriesDictionary();

            var replaceDictionary = new Dictionary<string, string>();
            if (File.Exists(replaceTSVPath))
            {
                Console.WriteLine("Loading replace dictionary from: " + replaceTSVPath);
                var replaceTSV = CSVFunctions.ReadCSV(replaceTSVPath);
                foreach (var v in replaceTSV)
                {
                    var cols = v.Split('\t');
                    var key = cols[0].Trim();
                    var value = cols[1].Trim();
                    replaceDictionary[key] = value;
                }
            }
            else
            {
                Console.WriteLine("No replace dictionary file found: " + replaceTSVPath);
            }



            {
                var stream = DataProcessor.DBLPProcessorFunctions.StreamCustomerItem(xmlPath);
                var counter = 0;
                //XElement root = new XElement("dblp");

                foreach (var v in stream)
                {
                    if (counter % 1000000 == 0)
                    {
                        Console.WriteLine(counter);
                    }
                    var booktitleElement = v.Element("booktitle");
                    if (v.Name == "proceedings")
                    {

                        var ele = DBLPProceedings.BuildFromXML(v);


                        if (ele.BookTitle.Length == 0)
                            {
                                foreach (var key in replaceDictionary.Keys)
                                {
                                    if (key.Length > 0)
                                    {
                                        var fstChar = key[0];
                                        if (fstChar == '=')
                                        {
                                            var regex = new Regex(key.Substring(1));
                                            var match = regex.Match(ele.key);
                                            if (match.Success)
                                            {
                                                Console.WriteLine("Replacing booktitle: " + ele.key + " " + ele.Title + " -> " + replaceDictionary[key]);
                                                ele.BookTitle = replaceDictionary[key];
                                            }

                                        }
                                        else if (ele.key == key)
                                        {
                                            Console.WriteLine("Replacing booktitle: " + ele.key + " " + ele.Title + " -> " + replaceDictionary[key]);
                                            ele.BookTitle = replaceDictionary[key];
                                        }

                                    }

                                }


                            }


                        proceedingsSeriesDictionary.Add(ele);
                    }
                    else if (v.Name == "inproceedings")
                    {
                        var ele = DBLPElement.BuildFromXML(v);
                        var key = v.Element("crossref")?.Value ?? "";

                        if (key.Length > 0 && ele.DOI.Length > 0)
                        {
                            if (doiDictionary.ContainsKey(key))
                            {
                                doiDictionary[key].Add(ele.DOI);
                            }
                            else
                            {
                                doiDictionary[key] = new List<string>();
                                doiDictionary[key].Add(ele.DOI);
                            }
                        }
                        
                    }

                    counter++;
                }

            }

            foreach (var kvp in doiDictionary)
            {
                if (proceedingsSeriesDictionary.ContainsKey(kvp.Key))
                {
                    var proceedings = proceedingsSeriesDictionary.GetProceedings(kvp.Key);
                    proceedings.DOIList = kvp.Value.Distinct().ToList();
                }
            }

            return proceedingsSeriesDictionary;
        }

        public static void WriteDBLPElements(List<DBLPElement> dblpElements, string outputFilePath)
        {
            using (var writer = new StreamWriter(outputFilePath, false, Encoding.UTF8))
            {
                foreach (var element in dblpElements)
                {
                    string jsonLine = element.ToJSONLine();
                    writer.WriteLine(jsonLine);
                }
            }

        }
    }
}

