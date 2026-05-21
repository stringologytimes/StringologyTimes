using System.Text;
using System.IO.Compression;
using System.Collections.ObjectModel;
namespace DataProcessor
{
    class DataCiteExternalFoundDOICache
    {
        public static string GetCachePath(string dataFolderPath)
        {
            return dataFolderPath + "/auto_generated/cache/datacite_cache/found_external_doi.jsonl";
        }
        public static Dictionary<string, string> Load(string dataFolderPath)
        {
            var dataCiteExternalDicPath = GetCachePath(dataFolderPath);
            var dataCiteExternalDic = DataProcessor.DataCiteJSONLLoader.Load(dataCiteExternalDicPath);
            return dataCiteExternalDic;
        }

        public static async Task Build(string dataFolderPath, ReadOnlySet<string> doiSet, HashSet<string> unknownDOISet, string mailAddress)
        {
            var dataCiteExternalDic = DataProcessor.DataCiteExternalFoundDOICache.Load(dataFolderPath);
            var dataCiteDOIPrefixSet = DataProcessor.DataCiteDOIToGZFileCache.GetDOIPrefixSet(dataFolderPath);
            var foundDOICache = DataProcessor.DataCiteFoundDOICache.Load(dataFolderPath);

            var externalDOICandidateList = new List<string>();
            foreach (var doi in doiSet)
            {
                var doiPrefix = DOIFunctions.GetPrefix(doi);
                if (!foundDOICache.ContainsKey(doi) && dataCiteDOIPrefixSet.Contains(doiPrefix) && !dataCiteExternalDic.ContainsKey(doi) && !unknownDOISet.Contains(doi))
                {
                    externalDOICandidateList.Add(doi);
                }
            }


            var http = DataProcessor.DataCiteClient.CreateHttpClient(mailAddress);

            var dict = await DataProcessor.DataCiteBatch.GetDoisAsync(
                http, externalDOICandidateList,
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
                        unknownDOISet.Add(doi);
                    }
                }
                else
                {
                    unknownDOISet.Add(doi);
                }
            }

            JsonLib.Save(dataCiteExternalDic, GetCachePath(dataFolderPath));


        }
    }
}
