
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
    public class SemanticScholarResult
    {
        public string DOI { get; set; } = "";
        public int ErrorType { get; set; } = 0;
        public List<string> DOIReferences { get; set; } = new List<string>();
        public List<string> UnknownReferences { get; set; } = new List<string>();


        public static SemanticScholarResult ParseFromJSON(string json)
        {
            var result = new SemanticScholarResult();
            var jsonDict = JsonLib.CreateDictionaryFromJSONL(json);

            result.DOI = jsonDict["inputDoi"];

            if (jsonDict.ContainsKey("error"))
            {
                var errorValue = jsonDict["error"];
                if (errorValue == "empty_doi")
                {
                    result.ErrorType = 1;
                }
                else
                {
                    result.ErrorType = 2;
                }
            }
            else
            {
                result.ErrorType = 0;
                if (!jsonDict.ContainsKey("data"))
                {
                    result.ErrorType = 3;
                    return result;
                }


                var dataArray = JsonLib.CreateArrayFromJSONL(jsonDict["data"]);
                foreach (var data in dataArray)
                {
                    var dataElementDict = JsonLib.CreateDictionaryFromJSONL(data);
                    if (dataElementDict.ContainsKey("citedPaper"))
                    {
                        var citedPaperDict = JsonLib.CreateDictionaryFromJSONL(dataElementDict["citedPaper"]);
                        if (citedPaperDict.ContainsKey("externalIds"))
                        {
                            if (citedPaperDict["externalIds"] == "{}")
                            {
                                //result.DOIReferences.Add("#3");
                            }
                            else
                            {
                                var externalIdsDict = JsonLib.CreateDictionaryFromJSONL(citedPaperDict["externalIds"]);
                                if (externalIdsDict.ContainsKey("DOI"))
                                {
                                    result.DOIReferences.Add(externalIdsDict["DOI"].ToLower());
                                }
                                else if (externalIdsDict.ContainsKey("ArXiv"))
                                {
                                    result.DOIReferences.Add("10.48550/arxiv." + externalIdsDict["ArXiv"]);
                                }
                                else
                                {
                                    //Console.WriteLine(citedPaperDict["externalIds"]);
                                    result.UnknownReferences.Add(citedPaperDict["externalIds"]);
                                }

                            }
                        }
                        else
                        {
                            //result.DOIReferences.Add("#2");
                        }
                    }
                    else
                    {
                        //result.DOIReferences.Add("#1");
                    }
                }

            }
            return result;
        }
    }

}
