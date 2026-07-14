using System.Text;
using System.IO.Compression;
using System.Collections.ObjectModel;
namespace DataProcessor
{
    class CrossRefExternalCache
    {
        public static string GetCachePath(string dataFolderPath)
        {
            return dataFolderPath + "/auto_generated/cache/crossref_cache/small_cache/found_external_doi.jsonl";
        }
        public static Dictionary<string, string> Load(string dataFolderPath)
        {
            var crossRefExternalDicPath = GetCachePath(dataFolderPath);
            var crossRefExternalDic = DOIFunctions.BuildMapperDOIToJSONL(crossRefExternalDicPath);
            return crossRefExternalDic;
        }

        public static Dictionary<string, string> LoadNotFoundDictionary(string dataFolderPath)
        {
            var crossRefNotFoundDicPath = GetUnknownDOIFilePath(dataFolderPath);
            var crossRefNotFoundDic = CSVFunctions.ReadCSVAasDictionary(crossRefNotFoundDicPath);
            return crossRefNotFoundDic;
        }
        public static void SaveNotFoundDictionary(string dataFolderPath, Dictionary<string, string> crossRefNotFoundDic)
        {
            var crossRefNotFoundDicPath = GetUnknownDOIFilePath(dataFolderPath);
            CSVFunctions.WriteCSVAsDictionary(crossRefNotFoundDicPath, crossRefNotFoundDic);
        }


        public static string GetUnknownDOIFilePath(string dataFolderPath)
        {
            var directoryInfo = new DirectoryInfo(dataFolderPath + "/auto_generated/cache/crossref_cache/small_cache");
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }
            return dataFolderPath + "/auto_generated/cache/crossref_cache/small_cache/unknown_doi.tsv";
        }



        public static async Task Update(string dataFolderPath, IDictionary<string, DOICacheInfo> doiCacheInfoDict, string mailAddress)
        {
            Console.WriteLine("Building CrossRefExternalFoundDOICache");

            var crossRefExternalDic = DataProcessor.CrossRefExternalCache.Load(dataFolderPath);
            var crossRefNotFoundDic = DataProcessor.CrossRefExternalCache.LoadNotFoundDictionary(dataFolderPath);
            var foundDOICache = DataProcessor.CrossRefLocalCache.Load(dataFolderPath);


            var externalDOICandidateList = new List<string>();
            var currentDate = DateTime.Now.ToString("yyyy-MM");

            doiCacheInfoDict.Values.ToList().ForEach((v) =>
            {
                if (v.SourceCite == "CrossRef")
                {
                    if (!crossRefExternalDic.ContainsKey(v.DOI) && v.CacheCreatedDate != currentDate && !crossRefNotFoundDic.ContainsKey(v.DOI))
                    {
                        if (v.SourceStatus != "LocalCache")
                        {
                            externalDOICandidateList.Add(v.DOI);
                        }
                    }

                }
            });

            Console.WriteLine("ExternalDOICandidateList: " + externalDOICandidateList.Count);
            if(externalDOICandidateList.Count == 0)
            {
                return;
            }

            var map = await DataProcessor.CrossrefBulk.GetManyAsync(externalDOICandidateList, mailto: mailAddress);
            //var unknownDOIList = new List<string>();


            foreach (var (doi, json) in map)
            {
                if (json != null)
                {
                    var value = DataProcessor.JsonLib.GetValueFromJSONL(json, "message");
                    if (value != null)
                    {
                        crossRefExternalDic[doi] = value;
                    }
                }
                else
                {
                    var date = DateTime.Now.ToString("yyyy-MM");
                    crossRefNotFoundDic[doi] = date;
                }
            }

            JsonLib.Save(crossRefExternalDic, GetCachePath(dataFolderPath));
            SaveNotFoundDictionary(dataFolderPath, crossRefNotFoundDic);
        }

        public static void UpdateDOICache(IDictionary<string, DOICacheInfo> doiCacheInfoDict, string dataFolderPath)
        {
            var crossRefExternalDic = DataProcessor.CrossRefExternalCache.Load(dataFolderPath);

            doiCacheInfoDict.Values.ToList().ForEach((v) =>
            {
                var doiPrefix = DOIFunctions.GetPrefix(v.DOI);
                if (v.SourceCite == "CrossRef")
                {
                    if (crossRefExternalDic.ContainsKey(v.DOI))
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

    }
}