using System.Text;
using System.IO.Compression;
using System.Collections.ObjectModel;
namespace DataProcessor
{
    class CrossRefSmallCache
    {
        public Dictionary<string, string> localCacheDic = new Dictionary<string, string>();
        public Dictionary<string, string> externalCacheDic = new Dictionary<string, string>();
        public Dictionary<string, string> notFoundCacheDic = new Dictionary<string, string>();

        public void Load(string dataFolderPath)
        {
            localCacheDic = CrossRefLocalCache.Load(dataFolderPath);
            externalCacheDic = CrossRefExternalCache.Load(dataFolderPath);
            notFoundCacheDic = CrossRefExternalCache.LoadNotFoundDictionary(dataFolderPath);
        }

        public void Save(string dataFolderPath)
        {
            JsonLib.Save(localCacheDic, CrossRefLocalCache.GetCachePath(dataFolderPath));
            JsonLib.Save(externalCacheDic, CrossRefExternalCache.GetCachePath(dataFolderPath));

            var crossRefNotFoundDicPath = CrossRefExternalCache.GetUnknownDOIFilePath(dataFolderPath);
            CSVFunctions.WriteCSVAsDictionary(crossRefNotFoundDicPath, notFoundCacheDic);
        }

        public Dictionary<string, DOIElement> LoadSmallCache(string dataFolderPath,IDictionary<string, DOICacheInfo> doiCacheInfoDict)
        {
            var logFilePath = dataFolderPath + "/auto_generated/log/load_small_cache.log";
            var logFile = new StreamWriter(logFilePath, true);
            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : Start");


            //var crossRefDic = DataProcessor.CrossRefLocalCache.Load(dataFolderPath);
            //var crossRefExternalDic = DataProcessor.CrossRefExternalCache.Load(dataFolderPath);

            var mergedDic = new Dictionary<string, DOIElement>();

            doiCacheInfoDict.Values.ToList().ForEach((v) =>
            {
                if (v.SourceCite == "CrossRef")
                {
                    if (localCacheDic.ContainsKey(v.DOI))
                    {
                        var doiElement = CrossRefParser.Parse(localCacheDic[v.DOI]);
                        mergedDic[v.DOI] = doiElement;
                    }
                    else if (externalCacheDic.ContainsKey(v.DOI))
                    {
                        var doiElement = CrossRefParser.Parse(externalCacheDic[v.DOI]);
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