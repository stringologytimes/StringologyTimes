using System.Xml;
using System.Xml.Linq;
using System.IO;
namespace DBLPProcessor
{
    enum URLType
    {
        ArXiv,
        DOI,
        Other
    }

    class DoiToTagMapper
    {
        public static Dictionary<string, List<string>> CreateDoiToTagMapper(string doiListCsvPath, string doiToTagCsvPath)
        {
            var doiToTagMapper = new Dictionary<string, List<string>>();

            var doiListCsvLines = File.ReadAllLines(doiListCsvPath);
            foreach (var line in doiListCsvLines)
            {
                var cols = line.Split(",");
                var doi = cols[0].Trim();
                if (!doiToTagMapper.ContainsKey(doi))
                {
                    doiToTagMapper[doi] = new List<string>();
                }
            }

            var doiToTagCsvLines = File.ReadAllLines(doiToTagCsvPath);
            foreach (var line in doiToTagCsvLines)
            {
                var cols = line.Split(",");
                var doi = cols[0].Trim();
                for (int i = 1; i < cols.Length; i++)
                {
                    var tag = cols[i].Trim();
                    if (doiToTagMapper.ContainsKey(doi))
                    {
                        doiToTagMapper[doi].Add(tag);
                    }
                    else
                    {
                        doiToTagMapper[doi] = new List<string>();
                        doiToTagMapper[doi].Add(tag);
                    }
                    
                }
            }
            return doiToTagMapper;
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

                foreach (var nodetype in DBLPProcessor.DBLPProcessorFunctions.MainNodeType)
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
