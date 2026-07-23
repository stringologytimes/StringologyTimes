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



        public static async Task Update(string dataFolderPath, IDictionary<string, DOICacheInfo> doiCacheInfoDict, CrossRefSmallCache crossRefSmallCache, string mailAddress)
        {
            CommonFunctions.OutputSystemMessageFunction("Building CrossRefExternalFoundDOICache [START]");
            CommonFunctions.IncrementParagraphCounter();

           // var crossRefExternalDic = DataProcessor.CrossRefExternalCache.Load(dataFolderPath);
           // var crossRefNotFoundDic = DataProcessor.CrossRefExternalCache.LoadNotFoundDictionary(dataFolderPath);
            //var foundDOICache = DataProcessor.CrossRefLocalCache.Load(dataFolderPath);
            var doiAliasListMapper = CSVFunctions.ReadCSVAasDictionary(DummyCacheManager.GetDOIAliasListFilePath(dataFolderPath));


            var externalDOICandidateList = new List<string>();
            var currentDate = DateTime.Now.ToString("yyyy-MM");

            doiCacheInfoDict.Values.ToList().ForEach((v) =>
            {
                if (v.SourceCite == "CrossRef")
                {
                    if (!crossRefSmallCache.externalCacheDic.ContainsKey(v.DOI) && v.CacheCreatedDate != currentDate && !crossRefSmallCache.notFoundCacheDic.ContainsKey(v.DOI))
                    {
                        if (v.SourceStatus != "LocalCache")
                        {
                            externalDOICandidateList.Add(v.DOI);
                        }
                    }

                }
            });

            CommonFunctions.OutputSystemMessageFunction("ExternalDOICandidateList: " + externalDOICandidateList.Count);

            if (externalDOICandidateList.Count == 0)
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
                        var doiElement = CrossRefParser.Parse(value);
                        var mainDOI = doiElement.DOI;

                        doiElement.DOIAliasList.ForEach((v) => {
                            if (!doiAliasListMapper.ContainsKey(v)) {
                                doiAliasListMapper[v] = mainDOI;
                            }
                        });
                        
                        crossRefSmallCache.externalCacheDic[mainDOI] = value;
                    }
                    else
                    {
                        var date = DateTime.Now.ToString("yyyy-MM");
                        crossRefSmallCache.notFoundCacheDic[doi] = date;
                    }
                }
                else
                {
                    var date = DateTime.Now.ToString("yyyy-MM");
                    crossRefSmallCache.notFoundCacheDic[doi] = date;
                }
            }


            CSVFunctions.WriteCSVAsDictionary(DummyCacheManager.GetDOIAliasListFilePath(dataFolderPath), doiAliasListMapper);

            CommonFunctions.DecrementParagraphCounter();
            CommonFunctions.OutputSystemMessageFunction("Building CrossRefExternalFoundDOICache [END]");
        }

        public static void UpdateDOICache(IDictionary<string, DOICacheInfo> doiCacheInfoDict, CrossRefSmallCache crossRefSmallCache)
        {
            doiCacheInfoDict.Values.ToList().ForEach((v) =>
            {
                var doiPrefix = DOIFunctions.GetPrefix(v.DOI);
                if (v.SourceCite == "CrossRef")
                {
                    if (crossRefSmallCache.externalCacheDic.ContainsKey(v.DOI))
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