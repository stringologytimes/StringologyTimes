using System;
using System.IO;
using System.Text;
using System.Linq;
using CommandLine;
using System.Threading.Tasks;
using DataProcessor;
using System.Text.Json;
using System.Collections.ObjectModel;

namespace DataProcessor
{
    class DOIElementPreprocessor
    {

        public static string GetCachePath(string dataFolderPath)
        {
            return dataFolderPath + "/auto_generated/cache/doi_element.jsonl";
        }
        /*
        public static Dictionary<string, DOIElement> LoadSmallCache(string dataFolderPath)
        {
            var doiElementDict = new Dictionary<string, DOIElement>();

            var crossRefDic = DataProcessor.CrossRefPreprocessor.LoadSmallCache(dataFolderPath);
            var dataCiteDic = DataProcessor.DataCitePreprocessor.LoadSmallCache(dataFolderPath);

            crossRefDic.ToList().ForEach((v) =>
            {
                doiElementDict[v.Key] = v.Value;
            });
            dataCiteDic.ToList().ForEach((v) =>
            {
                doiElementDict[v.Key] = v.Value;
            });
            return doiElementDict;
        }
        */

        public static async Task BuildSmallCache(string dataFolderPath, string mailAddress, ReadOnlySet<string> doiSet, string cacheFileName)
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



            await DataProcessor.CrossRefCacheBuilder.UpdateSmallCache(dataFolderPath, doiSet, mailAddress);
            await DataProcessor.DataCitePreprocessor.BuildSmallCache(dataFolderPath, doiSet, mailAddress);

            DataProcessor.CrossRefCacheBuilder.UpdateSmallCacheUsingContainerTitle(dataFolderPath);
            DataProcessor.DataCitePreprocessor.UpdateSmallCacheUsingContainerTitle(dataFolderPath);


            var doiElementDict = new Dictionary<string, DOIElement>();

            var crossRefDic = DataProcessor.CrossRefCacheBuilder.LoadSmallCache(dataFolderPath);
            var dataCiteDic = DataProcessor.DataCitePreprocessor.LoadSmallCache(dataFolderPath);

            crossRefDic.ToList().ForEach((v) =>
            {
                doiElementDict[v.Key] = v.Value;
            });
            dataCiteDic.ToList().ForEach((v) =>
            {
                doiElementDict[v.Key] = v.Value;
            });




            //await SemanticScholarPreprocessor.PreprocessAll(doiElementDict, dataFolderPath);



            DOIElement.Save(doiElementDict, GetCachePath(dataFolderPath));


            CSVFunctions.WriteCSV(hashFileInfo.FullName, hashList);
            Console.WriteLine("Building SmallCache [END]");
        }

        public static Dictionary<string, DOIElement> BuildDOIElementDictionary(string dataFolderPath, ReadOnlySet<string> doiSet)
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