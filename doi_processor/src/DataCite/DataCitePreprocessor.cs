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



        public static void BuildBigCache(string dataFolderPath)
        {
            var dataCiteDoiListFolderPath = dataFolderPath + "/auto_generated/cache/datacite_cache/gzfile_to_doi";

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


        public static Dictionary<string, DOIElement> LoadSmallCache(string dataFolderPath)
        {
            var doiElementDict = new Dictionary<string, DOIElement>();

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

            var mapperFromTitleToDOI = new Dictionary<string, string>();
            doiElementDict.ToList().ForEach((v) =>
            {
                if (v.Value.Title.Length > 0)
                {
                    mapperFromTitleToDOI[v.Value.Title] = v.Value.DOI;
                }
            });

            doiElementDict.ToList().ForEach((v) =>
            {
                if (mapperFromTitleToDOI.ContainsKey(v.Value.ContainerTitle))
                {
                    v.Value.ContainerDOI = mapperFromTitleToDOI[v.Value.ContainerTitle];
                }
            });


            return doiElementDict;
        }


        public static async Task BuildSmallCache(string dataFolderPath, ReadOnlySet<string> doiSet, string mailAddress)
        {
            var dataCiteFolderInfo = DataCiteJSONLLoader.SearchDataCiteFolder(dataFolderPath + "/external");
            var dataCiteOtherCSVPath = DataCiteDOIToGZFileCache.GetOthersFilePath(dataFolderPath);

            var dataCiteOtherCSVFileInfo = new FileInfo(dataCiteOtherCSVPath);
            if (!dataCiteOtherCSVFileInfo.Exists)
            {
                throw new Exception("others.tsv not found");
            }

            DataCiteFoundDOICache.Update(doiSet.ToList(), dataFolderPath, dataCiteFolderInfo.FullName);

            var unknownDOISet = CSVFunctions.ReadCSVAsHashSet(dataFolderPath + "/auto_generated/cache/datacite_cache/unknown_doi.tsv");
            await DataCiteExternalFoundDOICache.Build(dataFolderPath, doiSet, unknownDOISet, mailAddress);
            CSVFunctions.WriteCSV(dataFolderPath + "/auto_generated/cache/datacite_cache/unknown_doi.tsv", unknownDOISet);





        }


/*
        public static void BuildBookCache(string doiListFolderPath, string dataFolderPath)
        {
            Dictionary<string, StreamWriter> onlineWriters = new Dictionary<string, StreamWriter>();

            // gzファイル毎の処理を並列化し、各dict.Countを配列に格納
            var csvFiles = System.IO.Directory.GetFiles(doiListFolderPath, "*.tsv", System.IO.SearchOption.TopDirectoryOnly);
            //Dictionary<string, HashSet<string>> r = new Dictionary<string, HashSet<string>>();
            var hashSet = new HashSet<string>();

            var FinishedCounter = 0;
            var parallelCounter = 0;
            object lockObj = new object();

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 8 // 最大並列度を4に制限
            };

            System.Threading.Tasks.Parallel.For(0, csvFiles.Length, options, i =>
            {
                var csvFilePath = csvFiles[i];

                FileInfo fi = new FileInfo(csvFilePath);
                lock (lockObj)
                {
                    parallelCounter++;

                }
                var csvFileName = System.IO.Path.GetFileNameWithoutExtension(fi.Name);
                var splits = csvFileName.Split("_");
                var directoryName = splits[0] + "_" + splits[1];
                var gzFileName = splits[2] + "_" + splits[3];

                var doi_and_types = File.ReadAllLines(fi.FullName);
                lock (lockObj)
                {

                    foreach (var doi_and_type_line in doi_and_types)
                    {
                        var cols = doi_and_type_line.Split("\t");
                        var doi = cols[0];
                        var type = cols[1];

                        hashSet.Add(type);
                    }
                    FinishedCounter++;
                    parallelCounter--;

                    if (FinishedCounter % 1000 == 0)
                    {
                        Console.WriteLine("\t Processing: " + FinishedCounter + " / " + csvFiles.Length + " / ");
                    }

                }

            });

            Console.WriteLine("DataCite Book Cache: " + hashSet.Count);

            foreach (var type in hashSet)
            {
                Console.WriteLine("DataCite: " + type);
            }


        }
        */

        public static void UpdateSmallCacheUsingContainerTitle(string dataFolderPath)
        {
            var logFilePath = dataFolderPath + "/auto_generated/log/datacite_update_small_cache_using_container_title.log";
            var logFileInfo = new FileInfo(logFilePath);
            var logFile = new StreamWriter(logFilePath, true);
            logFile.Write(new DateTime().ToString("yyyy-MM-dd HH:mm:ss") + " : Start");



            //var dataCiteDicFromContainerTitleToDOI = DataProcessor.DataCiteDOIToGZFileCache.BuildDictionaryFromContainerTitleToDOI(dataFolderPath);

            var additionalDataCiteDOISet = new HashSet<string>();

            var typeHashSet = new HashSet<string>();

            var doiElementDict = LoadSmallCache(dataFolderPath);
            doiElementDict.ToList().ForEach((v) =>
            {
                typeHashSet.Add(v.Value.Type);
                if (v.Value.ContainerDOI.Length > 0 && !additionalDataCiteDOISet.Contains(v.Value.ContainerDOI))
                {
                    additionalDataCiteDOISet.Add(v.Value.ContainerDOI);
                    logFile.WriteLine($"Added DataCite DOI: {v.Value.ContainerDOI}");
                }
                if(v.Value.ContainerDOI.Length == 0 && (v.Value.Type == "ProceedingsArticle" || v.Value.Type == "ConferenceProceeding" || v.Value.Type == "JournalArticle"))
                {
                    logFile.WriteLine("No container DOI: " + v.Value.DOI + " / " + v.Value.Type + " / " + v.Value.ContainerTitle);
                }
            });
            logFile.Close();

            foreach (var type in typeHashSet)
            {
                Console.WriteLine("DataCite: " + type);
            }



            var dataCiteFolderInfo = DataCiteJSONLLoader.SearchDataCiteFolder(dataFolderPath + "/external");
            DataProcessor.DataCiteFoundDOICache.Update(additionalDataCiteDOISet.ToList(), dataFolderPath, dataCiteFolderInfo.FullName);
        }
    }
}
