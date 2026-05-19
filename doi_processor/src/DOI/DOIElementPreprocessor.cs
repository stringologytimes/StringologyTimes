using System;
using System.IO;
using System.Text;
using System.Linq;
using CommandLine;
using System.Threading.Tasks;
using DataProcessor;
using System.Text.Json;

namespace DataProcessor
{
    class DOIElementPreprocessor
    {

        public static string GetCachePath(string dataFolderPath)
        {
            return dataFolderPath + "/auto_generated/cache/doi_element.jsonl";
        }

        public static async Task BuildSmallCache(string dataFolderPath, string mailAddress, HashSet<string> doiSet, string cacheFileName)
        {
            var logFolderPath = dataFolderPath + "/auto_generated/log";

            var hashFileInfo = new FileInfo(dataFolderPath + "/auto_generated/cache/" + cacheFileName);
            Console.WriteLine("Building SmallCache [START]");

            List<string> hashList = new List<string>();
            hashList.Add(HashFunctions.ComputeHash(doiSet));
            var date = DateTime.Now;
            hashList.Add(date.ToString("yyyy-MM"));

            if (hashFileInfo.Exists)
            {
                var oldHashList = CSVFunctions.ReadCSV(hashFileInfo.FullName);
                if (oldHashList.Count == 2 && oldHashList[0] == hashList[0] && oldHashList[1] == hashList[1])
                {
                    Console.WriteLine("SmallCache already exists [END]");
                    return;
                }
            }



            await DataProcessor.CrossRefPreprocessor.BuildSmallCache(dataFolderPath, doiSet, mailAddress);
            await DataProcessor.DataCitePreprocessor.BuildSmallCache(dataFolderPath, doiSet, mailAddress);

            var doiElementDict = DOIElement.Load(GetCachePath(dataFolderPath), false);
            var crossRefDic = DataProcessor.CrossRefFoundDOICache.Load(dataFolderPath);
            crossRefDic.ToList().ForEach((v) =>
            {
                var doiElement = DOIElement.ParseFromCrossRefJSONL(v.Value, logFolderPath);
                doiElementDict[v.Key] = doiElement;
            });
            var crossRefExternalDic = DataProcessor.CrossRefExternalFoundDOICache.Load(dataFolderPath);
            crossRefExternalDic.ToList().ForEach((v) =>
            {
                var doiElement = DOIElement.ParseFromCrossRefJSONL(v.Value, logFolderPath);
                doiElementDict[v.Key] = doiElement;
            });
            var dataCiteDic = DataProcessor.DataCiteFoundDOICache.Load(dataFolderPath);
            dataCiteDic.ToList().ForEach((v) =>
            {
                var doiElement = DOIElement.ParseFromDataCiteJSONL(v.Value);
                doiElementDict[v.Key] = doiElement;
            });
            var dataCiteExternalDic = DataProcessor.DataCiteExternalFoundDOICache.Load(dataFolderPath);
            dataCiteExternalDic.ToList().ForEach((v) =>
            {
                var doiElement = DOIElement.ParseFromDataCiteJSONL(v.Value);
                doiElementDict[v.Key] = doiElement;
            });

            await SemanticScholarPreprocessor.PreprocessAll(doiElementDict, dataFolderPath);


            DOIElement.Save(doiElementDict, GetCachePath(dataFolderPath));


            CSVFunctions.WriteCSV(hashFileInfo.FullName, hashList);
            Console.WriteLine("Building SmallCache [END]");
        }

        public static Dictionary<string, DOIElement> BuildDOIElementDictionary(string dataFolderPath, HashSet<string> doiSet)
        {
            var doiElementDict = new Dictionary<string, DOIElement>();
            var doiElementCachePath = DOIElementPreprocessor.GetCachePath(dataFolderPath);
            var doiElementCache = DOIElement.Load(doiElementCachePath, false);
            doiSet.ToList().ForEach((v) =>
            {
                if (doiElementCache.ContainsKey(v))
                {
                    doiElementDict[v] = doiElementCache[v];
                }
            });
            return doiElementDict;
        }



    }
}