using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace DBLPProcessor
{
    class Processor
    {
        public static void Process(string xmlPath, string urlListPath, string outputPath)
        {

            HashSet<string> urlHashSet = new HashSet<string>();
            HashSet<string> journalURLHashSet = new HashSet<string>();
            HashSet<string> ProceedingNameHashSet = new HashSet<string>();

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
                    var url = "https://doi.org/" + line;
                    //Console.WriteLine(url + " / " + line);
                    urlHashSet.Add(url.ToLower());
                }
                else if (line.Length > 3)
                {
                    //Console.WriteLine(line);

                    urlHashSet.Add(line.ToLower());
                }
            }

            var stream = DBLPProcessor.DBLPProcessorFunctions.StreamCustomerItem(xmlPath);
            var counter = 0;
            XElement root = new XElement("dblp");

            foreach (var v in stream)
            {
                if (counter % 100000 == 0)
                {
                    Console.WriteLine(counter);
                }
                var b1 = v.Name == "inproceedings" && ProceedingNameHashSet.Contains(v.Element("booktitle")!.Value);
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
                        var url = eeChild.Value;
                        var formalURL = DBLPProcessor.URLTypeFunctions.getFormalURL(url);
                        /*
                        if (formalURL.IndexOf(@"LIPICS.CPM.2019") != -1)
                        {
                            Console.WriteLine("Found: " + formalURL);
                        }
                        */

                        if (urlHashSet.Contains(formalURL))
                        {
                            root.Add(v);
                            urlHashSet.Remove(formalURL);
                            Console.WriteLine(formalURL);
                            break;
                        }
                        if (b1 || b2)
                        {
                            root.Add(v);
                            Console.WriteLine(formalURL);
                            break;
                        }

                    }
                }
                counter++;
            }

            /*
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var outputDir = baseDir + "output";
            DirectoryInfo dirInfo = new DirectoryInfo(outputDir);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
            */

            root.Save(outputPath);
            Console.WriteLine("Saved: " + outputPath);

            foreach (var url in urlHashSet)
            {
                Console.WriteLine("Not found: " + url);
            }

        }
    }
}

