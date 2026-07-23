using System.Text;
using System.IO.Compression;
using System.Collections.ObjectModel;
namespace DataProcessor
{
    class DataCiteSmallCache
    {
        public Dictionary<string, string> localCacheDic = new Dictionary<string, string>();
        public Dictionary<string, string> externalCacheDic = new Dictionary<string, string>();
        public Dictionary<string, string> notFoundCacheDic = new Dictionary<string, string>();

        public void Load(string dataFolderPath)
        {
            localCacheDic = DataCiteLocalCache.Load(dataFolderPath);
            externalCacheDic = DataCiteExternalFoundDOICache.Load(dataFolderPath);
            notFoundCacheDic = DataCiteExternalFoundDOICache.LoadNotFoundDictionary(dataFolderPath);
        }

        public void Save(string dataFolderPath)
        {
            JsonLib.Save(localCacheDic, DataCiteLocalCache.GetCachePath(dataFolderPath));
            JsonLib.Save(externalCacheDic, DataCiteExternalFoundDOICache.GetCachePath(dataFolderPath));

            var dataCiteNotFoundDicPath = DataCiteExternalFoundDOICache.GetUnknownDOIFilePath(dataFolderPath);
            CSVFunctions.WriteCSVAsDictionary(dataCiteNotFoundDicPath, notFoundCacheDic);
        }


        public Dictionary<string, DOIElement> LoadSmallCache(string dataFolderPath, IDictionary<string, DOICacheInfo> doiCacheInfoDict)
        {
            var logFilePath = dataFolderPath + "/auto_generated/log/load_small_cache.log";
            var logFile = new StreamWriter(logFilePath, true);
            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : Start");


            var mergedDic = new Dictionary<string, DOIElement>();

            doiCacheInfoDict.Values.ToList().ForEach((v) =>
            {
                if (v.SourceCite == "DataCite")
                {
                    if (localCacheDic.ContainsKey(v.DOI))
                    {
                        var doiElement = DataCiteParser.Parse(localCacheDic[v.DOI]);
                        mergedDic[v.DOI] = doiElement;
                    }
                    else if (externalCacheDic.ContainsKey(v.DOI))
                    {
                        var doiElement = DataCiteParser.Parse(externalCacheDic[v.DOI]);
                        mergedDic[v.DOI] = doiElement;
                    }
                    else
                    {
                        var doiElement = new DOIElement() { DOI = v.DOI, Source = "CrossRef", IsPrimary = v.DOIRank == 0 };
                        mergedDic[v.DOI] = doiElement;
                    }
                }
            });

            mergedDic.ToList().ForEach((v) =>
            {
                v.Value.UpdateContainerDOI(doiCacheInfoDict[v.Key]);
            });


            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : End");
            logFile.Close();
            return mergedDic;

        }
    }
}