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

        public static string GetUnknownDOIFilePath(string dataFolderPath)
        {
            var directoryInfo = new DirectoryInfo(dataFolderPath + "/auto_generated/cache/datacite_cache/small_cache");
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }
            return dataFolderPath + "/auto_generated/cache/datacite_cache/small_cache/unknown_doi.tsv";
        }


        public static Dictionary<string, string> Load(string dataFolderPath)
        {
            var dataCiteExternalDicPath = GetCachePath(dataFolderPath);
            return JsonLib.LoadJSONLAsDictionary(dataCiteExternalDicPath, "id");
        }


        public static Dictionary<string, string> LoadNotFoundDictionary(string dataFolderPath)
        {
            var crossRefNotFoundDicPath = GetUnknownDOIFilePath(dataFolderPath);
            var crossRefNotFoundDic = CSVFunctions.ReadCSVAasDictionary(crossRefNotFoundDicPath);
            return crossRefNotFoundDic;
        }

        public static void SaveNotFoundDictionary(string dataFolderPath, Dictionary<string, string> dataCiteNotFoundDic)
        {
            var dicPath = GetUnknownDOIFilePath(dataFolderPath);
            CSVFunctions.WriteCSVAsDictionary(dicPath, dataCiteNotFoundDic);
        }

        public static void UpdateDOICache(IDictionary<string, DOICacheInfo> doiCacheInfoDict, DataCiteSmallCache dataCiteSmallCache)
        {

            doiCacheInfoDict.Values.ToList().ForEach((v) =>
            {
                //var doiPrefix = DOIFunctions.GetPrefix(v.DOI);
                if (v.SourceCite == "DataCite")
                {
                    if (dataCiteSmallCache.externalCacheDic.ContainsKey(v.DOI))
                    {
                        v.SourceStatus = "ExternalCache";
                        v.CacheCreatedDate = DateTime.Now.ToString("yyyy-MM");
                    }
                    else
                    {
                        if (v.SourceStatus != "LocalCache")
                        {
                            v.SourceStatus = "NotFound";
                            v.CacheCreatedDate = DateTime.Now.ToString("yyyy-MM");
                        }
                    }
                }
            });
        }
        public static async Task Build(string dataFolderPath, IDictionary<string, DOICacheInfo> doiCacheInfoDict, DataCiteSmallCache dataCiteSmallCache, string mailAddress)
        {
            CommonFunctions.OutputSystemMessageFunction("Building DataCite External Found DOI Cache [START]");
            CommonFunctions.IncrementParagraphCounter();


            var externalDOICandidateList = new List<string>();
            var dateNowStr = DateTime.Now.ToString("yyyy-MM");
            doiCacheInfoDict.Values.ToList().ForEach((v) =>
                {
                    if (v.SourceCite == "DataCite")
                    {
                        if (!dataCiteSmallCache.externalCacheDic.ContainsKey(v.DOI) && v.CacheCreatedDate != dateNowStr && !dataCiteSmallCache.notFoundCacheDic.ContainsKey(v.DOI))
                        {
                            if (v.SourceStatus != "LocalCache")
                            {
                                externalDOICandidateList.Add(v.DOI);
                            }
                        }
                    }
                });

            CommonFunctions.OutputSystemMessageFunction("Found " + externalDOICandidateList.Count + " external DOI candidates");

            if (externalDOICandidateList.Count == 0)
            {
                return;
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
                        dataCiteSmallCache.externalCacheDic[doi] = value;
                    }
                    else
                    {

                        var date = DateTime.Now.ToString("yyyy-MM");
                        dataCiteSmallCache.notFoundCacheDic[doi] = date;
                    }
                }
                else
                {

                    var date = DateTime.Now.ToString("yyyy-MM");
                    dataCiteSmallCache.notFoundCacheDic[doi] = date;
                }
            }

            CommonFunctions.DecrementParagraphCounter();
            CommonFunctions.OutputSystemMessageFunction("Building DataCite External Found DOI Cache [END]");

        }
    }
}
