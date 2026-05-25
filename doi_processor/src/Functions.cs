using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Collections.ObjectModel;

namespace DataProcessor
{
    enum URLType
    {
        ArXiv,
        DOI,
        Other
    }

    class HashFunctions
    {
        public static string ComputeHash(ReadOnlySet<string> doiSet)
        {
            List<string> doiList = doiSet.ToList();
            doiList.Sort();
            string str = "";
            foreach (var doi in doiList)
            {
                str += doi;
            }


            byte[] bytes = Encoding.UTF8.GetBytes(str);

            using SHA256 sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(bytes);

            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

    }

    class DoiToTagMapper
    {
        public static HashSet<string> CollectSpecialContainerDOI(string dataFolderPath, IList<string> doiList, string logFilePath)
        {
            var logFile = new StreamWriter(logFilePath, true);
            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : Start");


            var doiPrefixSettingPath = dataFolderPath + "/raw/small_cache_setting/doi_prefix.tsv";
            var doiPrefixDictionary = CSVFunctions.ReadCSVAasDictionary(doiPrefixSettingPath);
            var specialContainerDOIList = new List<string>();

            var additionalCrossRefDOISet = new HashSet<string>();

            doiList.ToList().ForEach((doi) =>
            {
                doiPrefixDictionary.ToList().ForEach((w) =>
                {
                    var regexMatchResult = SpecialRegexMatchResult.Match(w.Key, doi, w.Value);
                    if (regexMatchResult.IsMatch && regexMatchResult.NewValue != null)
                    {
                        if (!additionalCrossRefDOISet.Contains(regexMatchResult.NewValue))
                        {
                            logFile.WriteLine("Matched DOI: " + doi + " -> " + regexMatchResult.NewValue);
                        }
                        additionalCrossRefDOISet.Add(regexMatchResult.NewValue);
                    }
                });
            });

            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : End");
            logFile.Close();

            return additionalCrossRefDOISet;

        }
        public static Dictionary<string, List<string>> CreateDoiToTagMapper(string dataRawFolderPath)
        {
            var doiToTagMapper = new Dictionary<string, HashSet<string>>();
            DirectoryInfo directoryInfo = new DirectoryInfo(dataRawFolderPath);
            // directoryInfo中のFileInfoを再帰的に列挙
            var files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file.FullName);

                if (fileInfo.Name == "url_and_doi.csv" || fileInfo.Name == "url_and_doi.tsv")
                {
                    var lines = File.ReadAllLines(fileInfo.FullName);
                    foreach (var line in lines)
                    {
                        var cols = line.Split(",");
                        var doi = cols[0].Trim().ToLower();
                        if (!doiToTagMapper.ContainsKey(doi))
                        {
                            doiToTagMapper[doi] = new HashSet<string>();
                        }
                    }
                }
                if (fileInfo.Name == "tag.csv" || fileInfo.Name == "tag.tsv")
                {
                    var lines = File.ReadAllLines(fileInfo.FullName);
                    foreach (var line in lines)
                    {
                        var splitChar = fileInfo.Extension == ".csv" ? "," : "\t";
                        var cols = line.Split(splitChar);
                        var doi = cols[0].Trim().ToLower();
                        if (!doiToTagMapper.ContainsKey(doi))
                        {
                            doiToTagMapper[doi] = new HashSet<string>();
                        }
                        for (int i = 1; i < cols.Length; i++)
                        {
                            var tag = cols[i].Trim();
                            doiToTagMapper[doi].Add(tag);
                            //Console.WriteLine($"DOI: {doi} -> {tag}");
                        }
                    }
                }
            }

            var doiToTagMapper2 = new Dictionary<string, List<string>>();
            foreach (var doi in doiToTagMapper.Keys)
            {
                doiToTagMapper2[doi] = doiToTagMapper[doi].ToList();
            }
            return doiToTagMapper2;
        }
    }





    class DBLPProcessorFunctions
    {
        public static string[] MainNodeType = new string[] { "article", "inproceedings", "proceedings", "book", "incollection", "phdthesis", "masterthesis", "www" };

        public static IEnumerable<XElement> StreamCustomerItem(string uri)
        {
            XmlReaderSettings settings = new XmlReaderSettings();

            // SET THE RESOLVER
            settings.XmlResolver = new XmlUrlResolver();
            settings.ValidationType = ValidationType.DTD;
            settings.DtdProcessing = DtdProcessing.Parse;
            settings.IgnoreWhitespace = true;

            using (XmlReader reader = XmlReader.Create(uri, settings))
            {
                //XElement name = null;

                reader.MoveToContent();
                HashSet<string> mainNodeNames = new HashSet<string>();

                foreach (var nodetype in DataProcessor.DBLPProcessorFunctions.MainNodeType)
                {
                    mainNodeNames.Add(nodetype);
                }

                int n = 0;
                while (!reader.EOF)
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        var name = reader.Name;
                        if (mainNodeNames.Contains(name))
                        {
                            var node = XElement.ReadFrom(reader) as XElement;
                            if (node != null)
                            {
                                yield return node;
                            }
                        }
                        else
                        {
                            reader.Read();
                        }
                        n++;
                    }
                    else
                    {
                        reader.Read();
                    }

                }
            }
        }

    }

}
