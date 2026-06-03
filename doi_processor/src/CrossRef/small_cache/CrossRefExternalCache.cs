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
            var foundDOICache = DataProcessor.CrossRefLocalCache.Load(dataFolderPath);


            var externalDOICandidateList = new List<string>();
            var currentDate = DateTime.Now.ToString("yyyy-MM");

            doiCacheInfoDict.Values.ToList().ForEach((v) =>
            {
                if (v.SourceCite == "CrossRef")
                {
                    if (!crossRefExternalDic.ContainsKey(v.DOI) && v.Date != currentDate)
                    {
                        if (v.SourceStatus != "LocalCache")
                        {
                            externalDOICandidateList.Add(v.DOI);
                        }
                    }

                }



            });

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
            }

            JsonLib.Save(crossRefExternalDic, GetCachePath(dataFolderPath));
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

    }
}