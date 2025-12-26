using System;
using System.IO;
using System.Text;
using System.Linq;
using CommandLine;
using System.Threading.Tasks;
namespace DataProcessor
{
    class FoundOrNotFoundLists
    {
        public List<string> NotFoundDois { get; set; } = new List<string>();
        public List<string> NotFoundCrossRefDois { get; set; } = new List<string>();
        public List<string> NotFoundDataCiteDois { get; set; } = new List<string>();
        public List<string> FoundDois { get; set; } = new List<string>();
        /*
        public List<string> FoundCrossRefDois { get; set; } = new List<string>();
        public List<string> FoundDataCiteDois { get; set; } = new List<string>();
        */

    }
    class CommonPreprocessors
    {
        public static FoundOrNotFoundLists CreateFoundOrNotFoundLists(string dataFolderPath, Dictionary<string, List<string>> doiToTagMapper,
        Dictionary<string, string> crossRefDic,
        Dictionary<string, string> dataCiteDic,
        Dictionary<string, string> crossRefExternalDic,
        Dictionary<string, string> dataCiteExternalDic,
        HashSet<string> crossRefDOIPrefixSet,
        HashSet<string> dataCiteDOIPrefixSet)

        {
            var foundOrNotFoundLists = new FoundOrNotFoundLists();

            foreach (var doi in doiToTagMapper.Keys)
            {
                var b1 = crossRefDic.ContainsKey(doi);
                var b2 = dataCiteDic.ContainsKey(doi);
                var b3 = crossRefExternalDic.ContainsKey(doi);
                var b4 = dataCiteExternalDic.ContainsKey(doi);

                if (b1 || b2 || b3 || b4)
                {
                    foundOrNotFoundLists.FoundDois.Add(doi);
                }
                else
                {
                    var doiPrefix = DataProcessor.DOIFunctions.GetPrefix(doi);

                    if (crossRefDOIPrefixSet.Contains(doiPrefix))
                    {
                        foundOrNotFoundLists.NotFoundCrossRefDois.Add(doi);
                    }
                    else if (dataCiteDOIPrefixSet.Contains(doiPrefix))
                    {
                        foundOrNotFoundLists.NotFoundDataCiteDois.Add(doi);
                    }
                    else
                    {
                        foundOrNotFoundLists.NotFoundDois.Add(doi);
                    }
                }
            }
            return foundOrNotFoundLists;
        }

        public async static void ExternalSearch(FoundOrNotFoundLists foundOrNotFoundLists, string mailAddress, Dictionary<string, string> crossRefExternalDic, Dictionary<string, string> dataCiteExternalDic)
        {
            var map = await DataProcessor.CrossrefBulk.GetManyAsync(foundOrNotFoundLists.NotFoundCrossRefDois, mailto: mailAddress);

            foreach (var (doi, json) in map)
            {
                if (json != null)
                {
                    var value = DataProcessor.JsonLib.GetValueFromJSONL(json, "message");
                    if (value != null)
                    {
                        crossRefExternalDic[doi] = value;
                    }
                    else
                    {
                        foundOrNotFoundLists.NotFoundCrossRefDois.Add(doi);
                    }
                }
                else
                {
                    foundOrNotFoundLists.NotFoundCrossRefDois.Add(doi);
                }
            }


            var http = DataProcessor.DataCiteClient.CreateHttpClient(mailAddress);


            var dict = await DataProcessor.DataCiteBatch.GetDoisAsync(
                http, foundOrNotFoundLists.NotFoundDataCiteDois,
                maxConcurrency: 4,
                requestsPerSecond: 2.5);

            foreach (var (doi, json) in dict)
            {
                if (json != null)
                {
                    var value = DataProcessor.JsonLib.GetValueFromJSONL(json.RootElement.GetRawText(), "data");
                    if (value != null)
                    {
                        dataCiteExternalDic[doi] = value;
                    }
                    else
                    {
                        foundOrNotFoundLists.NotFoundDataCiteDois.Add(doi);
                    }
                }
                else
                {
                    foundOrNotFoundLists.NotFoundDataCiteDois.Add(doi);
                }
            }
        }

    }
}
