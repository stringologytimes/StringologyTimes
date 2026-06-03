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
        public static void UpdateDOICache(IDictionary<string, DOICacheInfo> doiCacheInfoDict, string dataFolderPath)
        {
            var dataCiteExternalDic = DataProcessor.DataCiteExternalFoundDOICache.Load(dataFolderPath);

            doiCacheInfoDict.Values.ToList().ForEach((v) =>
            {
                //var doiPrefix = DOIFunctions.GetPrefix(v.DOI);
                if (v.SourceCite == "DataCite")
                {
                    if (dataCiteExternalDic.ContainsKey(v.DOI))
                    {
                        v.SourceStatus = "ExternalCache";
                        v.Date = DateTime.Now.ToString("yyyy-MM");
                    }
                    else
                    {
                        if (v.SourceStatus != "LocalCache")
                        {
                            v.SourceStatus = "NotFound";
                            v.Date = DateTime.Now.ToString("yyyy-MM");
                        }
                    }
                }
            });
        }
        public static async Task Build(string dataFolderPath, IDictionary<string, DOICacheInfo> doiCacheInfoDict, string mailAddress)
        {
            var dataCiteExternalDic = DataProcessor.DataCiteExternalFoundDOICache.Load(dataFolderPath);
            var foundDOICache = DataProcessor.DataCiteLocalCache.Load(dataFolderPath);

            var externalDOICandidateList = new List<string>();
            var dateNowStr = DateTime.Now.ToString("yyyy-MM");
            doiCacheInfoDict.Values.ToList().ForEach((v) =>
                {
                    if (v.SourceCite == "DataCite")
                    {
                        if (!dataCiteExternalDic.ContainsKey(v.DOI) && v.Date != dateNowStr)
                        {
                            if (v.SourceStatus != "LocalCache")
                            {
                                externalDOICandidateList.Add(v.DOI);
                            }
                        }
                    }
                });


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
                }
            }

            JsonLib.Save(dataCiteExternalDic, GetCachePath(dataFolderPath));


        }
    }
}
