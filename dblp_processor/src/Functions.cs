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
    class URLTypeFunctions
    {
        public static URLType getURLType(string url)
        {
            if (url.IndexOf("http://arxiv.org/") == 0)
            {
                return URLType.ArXiv;
            }
            else if (url.IndexOf("https://arxiv.org/") == 0)
            {
                return URLType.ArXiv;
            }
            else if (url.IndexOf("https://doi.org/") == 0)
            {
                return URLType.DOI;
            }
            else if (url.IndexOf("http://doi.org/") == 0)
            {
                return URLType.DOI;
            }
            else
            {
                return URLType.Other;
            }
        }
        public static string getFormalURL(string dblpURL)
        {
            if (dblpURL.IndexOf("http://arxiv.org/") == 0)
            {
                var i = "http://arxiv.org/".Length;
                return ("https://arxiv.org/" + dblpURL.Substring(i)).ToLower();
            }
            else if (dblpURL.IndexOf("https://arxiv.org/") == 0)
            {
                return dblpURL.ToLower();
            }
            else if (dblpURL.IndexOf("https://doi.org/") == 0)
            {
                return dblpURL.ToLower();
            }
            else if (dblpURL.IndexOf("http://doi.org/") == 0)
            {
                var i = "http://doi.org/".Length;
                return ("https://doi.org/" + dblpURL.Substring(i)).ToLower();
            }
            else
            {
                return dblpURL.ToLower();
            }

        }

    }




}
