using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Collections.ObjectModel;
namespace DataProcessor
{
    class DataCitePreprocessor
    {
        public static string GetUnknownDOIFilePath(string dataFolderPath)
        {
            var f = new DirectoryInfo(dataFolderPath + "/auto_generated/cache/datacite_cache/small_cache");
            if (!f.Exists)
            {
                f.Create();
            }
            return dataFolderPath + "/auto_generated/cache/datacite_cache/small_cache/unknown_doi.tsv";
        }



        public static void BuildBigCache(string dataFolderPath)
        {
            var dataCiteDoiListFolderPath = DataCiteGZFileToDOICache.GetFolderPath(dataFolderPath);

            var dataCiteFolderInfo = DataCiteJSONLLoader.SearchDataCiteFolder(dataFolderPath + "/external");

            DataProcessor.DataCiteGZFileToDOICache.Build(dataCiteFolderInfo.FullName, dataFolderPath);
            var dataCiteOtherCSVPath = DataCiteDOIToGZFileCache.GetOthersFilePath(dataFolderPath);
            var dataCiteOtherCSVFileInfo = new FileInfo(dataCiteOtherCSVPath);
            if (!dataCiteOtherCSVFileInfo.Exists)
            {
                DataCiteDOIToGZFileCache.Build(dataCiteDoiListFolderPath, dataFolderPath);
            }
            //BuildBookCache(dataCiteDoiListFolderPath, dataFolderPath);
        }


        public static Dictionary<string, DOIElement> LoadSmallCache(string dataFolderPath, IDictionary<string, DOICacheInfo> doiCacheInfoDict, HashSet<string> dataCiteDOIPrefixSet)
        {
            var logFilePath = dataFolderPath + "/auto_generated/log/load_small_cache.log";
            var logFile = new StreamWriter(logFilePath, true);
            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : Start");

            var dataCiteDic = DataProcessor.DataCiteLocalCache.Load(dataFolderPath);
            var dataCiteExternalDic = DataProcessor.DataCiteExternalFoundDOICache.Load(dataFolderPath);

            var mergedDic = new Dictionary<string, DOIElement>();

            doiCacheInfoDict.Values.ToList().ForEach((v) =>
            {
                if (v.SourceCite == "DataCite")
                {
                    if (dataCiteDic.ContainsKey(v.DOI))
                    {
                        var doiElement = DataCiteParser.Parse(dataCiteDic[v.DOI]);
                        mergedDic[v.DOI] = doiElement;
                    }
                    else if (dataCiteExternalDic.ContainsKey(v.DOI))
                    {
                        var doiElement = DataCiteParser.Parse(dataCiteExternalDic[v.DOI]);
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

        public static async Task UpdateSmallCache(string dataFolderPath, IDictionary<string, DOICacheInfo> doiCacheInfoDict, string mailAddress)
        {
            Console.WriteLine("Building DataCiteSmallCache [START]");

            var dataCiteFolderInfo = DataCiteJSONLLoader.SearchDataCiteFolder(dataFolderPath + "/external");
            var dataCiteOtherCSVPath = DataCiteDOIToGZFileCache.GetOthersFilePath(dataFolderPath);

            var dataCiteOtherCSVFileInfo = new FileInfo(dataCiteOtherCSVPath);
            if (!dataCiteOtherCSVFileInfo.Exists)
            {
                throw new Exception("others.tsv not found");
            }

            // Build Found DOI Cache

            DataCiteLocalCache.Update(doiCacheInfoDict, dataFolderPath, dataCiteFolderInfo.FullName);
            DataCiteLocalCache.UpdateDOICache(doiCacheInfoDict, dataFolderPath);
            await DataCiteExternalFoundDOICache.Build(dataFolderPath, doiCacheInfoDict, mailAddress);

            DataCiteExternalFoundDOICache.UpdateDOICache(doiCacheInfoDict, dataFolderPath);


            Console.WriteLine("DataCiteSmallCache [END]");

        }





    }
}
