using System.Text;
using System.IO.Compression;
using System.Collections.ObjectModel;
namespace DataProcessor
{
    class DataCiteExternalFoundDOICache
    {
        public static string GetCachePath(string dataFolderPath)
        {
            var f = new DirectoryInfo(dataFolderPath + "/auto_generated/cache/datacite_cache/small_cache");
            if (!f.Exists)
            {
                f.Create();
            }

            return dataFolderPath + "/auto_generated/cache/datacite_cache/small_cache/found_external_doi.jsonl";
        }
        public static Dictionary<string, string> Load(string dataFolderPath)
        {
            var dataCiteExternalDicPath = GetCachePath(dataFolderPath);
            var dataCiteExternalDic = DataProcessor.DataCiteJSONLLoader.Load(dataCiteExternalDicPath);
            return dataCiteExternalDic;
        }

        public static async Task Build(string dataFolderPath, ReadOnlySet<string> doiSet, Dictionary<string, string> unknownDOIDictionary, string mailAddress)
        {
            var dataCiteExternalDic = DataProcessor.DataCiteExternalFoundDOICache.Load(dataFolderPath);
            var dataCiteDOIPrefixSet = DataProcessor.DataCiteDOIToGZFileCache.GetDOIPrefixSet(dataFolderPath);
            var foundDOICache = DataProcessor.DataCiteFoundDOICache.Load(dataFolderPath);

            var externalDOICandidateList = new List<string>();
            var dateNowStr = DateTime.Now.ToString("yyyy-MM");
            foreach (var doi in doiSet)
            {
                var doiPrefix = DOIFunctions.GetPrefix(doi);
                if (!foundDOICache.ContainsKey(doi) && dataCiteDOIPrefixSet.Contains(doiPrefix) && !dataCiteExternalDic.ContainsKey(doi))
                {
                    if (!unknownDOIDictionary.ContainsKey(doi))
                    {
                        externalDOICandidateList.Add(doi);
                    }
                    else
                    {
                        var foundDateStr = unknownDOIDictionary[doi];
                        if (dateNowStr != foundDateStr)
                        {
                            externalDOICandidateList.Add(doi);
                        }
                    }
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
                        if (unknownDOIDictionary.ContainsKey(doi))
                        {
                            unknownDOIDictionary.Remove(doi);
                        }
                    }
                    else
                    {
                        unknownDOIDictionary[doi] = dateNowStr;
                    }
                }
                else
                {
                    unknownDOIDictionary[doi] = dateNowStr;
                }
            }

            JsonLib.Save(dataCiteExternalDic, GetCachePath(dataFolderPath));


        }
    }
}
