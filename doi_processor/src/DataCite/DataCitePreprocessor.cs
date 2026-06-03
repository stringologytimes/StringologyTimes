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


            var doiElementDict = new Dictionary<string, DOIElement>();
            var dataCiteDic = DataProcessor.DataCiteLocalCache.Load(dataFolderPath);
            dataCiteDic.ToList().ForEach((v) =>
            {
                var doiElement = DataCiteParser.Parse(v.Value);
                doiElementDict[v.Key] = doiElement;
            });
            var dataCiteExternalDic = DataProcessor.DataCiteExternalFoundDOICache.Load(dataFolderPath);
            dataCiteExternalDic.ToList().ForEach((v) =>
            {
                var doiElement = DataCiteParser.Parse(v.Value);
                doiElementDict[v.Key] = doiElement;
            });

            var lambdaLoadFunction = (DOICacheInfo v, bool isPrimary) =>
            {
                var doiPrefix = DOIFunctions.GetPrefix(v.DOI);
                if (dataCiteDOIPrefixSet.Contains(doiPrefix))
                {
                    if (dataCiteDic.ContainsKey(v.DOI))
                    {
                        var r = DataCiteParser.Parse(dataCiteDic[v.DOI]);
                        r.IsPrimary = isPrimary;
                        if (v.ContainerDOI.Length == 0 && v.ContainerDOI.Length > 0)
                        {
                            r.ContainerDOI = v.ContainerDOI;
                        }
                        return r;
                    }
                    else if (dataCiteExternalDic.ContainsKey(v.DOI))
                    {
                        var r = DataCiteParser.Parse(dataCiteExternalDic[v.DOI]);
                        r.IsPrimary = isPrimary;
                        if (v.ContainerDOI.Length == 0 && v.ContainerDOI.Length > 0)
                        {
                            r.ContainerDOI = v.ContainerDOI;
                        }
                        return r;
                    }
                    else
                    {
                        var r = new DOIElement() { DOI = v.DOI, Source = "DataCite:NotFound" };
                        r.IsPrimary = isPrimary;
                        if (v.ContainerDOI.Length == 0 && v.ContainerDOI.Length > 0)
                        {
                            r.ContainerDOI = v.ContainerDOI;
                        }
                        return r;
                    }
                }
                else
                {
                    return null;
                }
            };

            doiCacheInfoDict.Values.ToList().ForEach((v) =>
            {
                var doiElement = lambdaLoadFunction(v, v.Priority == 0);
                if (doiElement != null)
                {
                    doiElementDict[v.DOI] = doiElement;
                }
            });

            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : End");
            logFile.Close();
            return doiElementDict;

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



        /*

                public static async Task UpdateSmallCache(string dataFolderPath, ReadOnlySet<string> doiSet, string mailAddress)
                {
                    var dataCiteFolderInfo = DataCiteJSONLLoader.SearchDataCiteFolder(dataFolderPath + "/external");
                    var dataCiteOtherCSVPath = DataCiteDOIToGZFileCache.GetOthersFilePath(dataFolderPath);

                    var dataCiteOtherCSVFileInfo = new FileInfo(dataCiteOtherCSVPath);
                    if (!dataCiteOtherCSVFileInfo.Exists)
                    {
                        throw new Exception("others.tsv not found");
                    }

                    DataCiteLocalCache.Update(doiSet.ToList(), dataFolderPath, dataCiteFolderInfo.FullName);

                    var unknownDOIDictionary = CSVFunctions.ReadCSVAasDictionary(GetUnknownDOIFilePath(dataFolderPath));
                    await DataCiteExternalFoundDOICache.Build(dataFolderPath, doiSet, unknownDOIDictionary, mailAddress);
                    CSVFunctions.WriteCSVAsDictionary(GetUnknownDOIFilePath(dataFolderPath), unknownDOIDictionary);

                }
                */


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

/*
        public static void UpdateSmallCacheUsingContainerTitle(string dataFolderPath)
        {

            var logFilePath = dataFolderPath + "/auto_generated/log/datacite_update_small_cache_using_container_title.log";
            var logFileInfo = new FileInfo(logFilePath);
            var logFile = new StreamWriter(logFilePath, true);
            logFile.WriteLine(new DateTime().ToString("yyyy-MM-dd HH:mm:ss") + " : Start");

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
                if (v.Value.ContainerDOI.Length == 0 && (v.Value.Type == "ProceedingsArticle" || v.Value.Type == "ConferenceProceeding" || v.Value.Type == "JournalArticle"))
                {
                    logFile.WriteLine("No container DOI: " + v.Value.DOI + " / " + v.Value.Type + " / " + v.Value.ContainerTitle);
                }
            });

            foreach (var type in typeHashSet)
            {
                logFile.WriteLine("DataCite: " + type);
            }
            logFile.Close();

            var dataCiteFolderInfo = DataCiteJSONLLoader.SearchDataCiteFolder(dataFolderPath + "/external");
            DataProcessor.DataCiteFoundDOICache.Update(additionalDataCiteDOISet.ToList(), dataFolderPath, dataCiteFolderInfo.FullName);

        }

        public static void UpdateSmallCacheUsingDOIPrefix(string dataFolderPath)
        {

            var logFilePath = dataFolderPath + "/auto_generated/log/datacite_update_small_cache_using_doi_prefix.log";
            var logFile = new StreamWriter(logFilePath, true);
            logFile.WriteLine(new DateTime().ToString("yyyy-MM-dd HH:mm:ss") + " : Start");

            var doiPrefixSettingPath = dataFolderPath + "/raw/small_cache_setting/doi_prefix.tsv";
            var doiPrefixDictionary = CSVFunctions.ReadCSVAasDictionary(doiPrefixSettingPath);

            var additionalCrossRefDOISet = new HashSet<string>();

            var doiElementDict = LoadSmallCache(dataFolderPath);
            doiElementDict.ToList().ForEach((v) =>
            {
                var doi = v.Value.DOI;
                doiPrefixDictionary.ToList().ForEach((w) =>
                {
                    var regexMatchResult = SpecialRegexMatchResult.Match(w.Key, doi, w.Value);
                    if (regexMatchResult.IsMatch && regexMatchResult.NewValue != null)
                    {
                        if (!additionalCrossRefDOISet.Contains(regexMatchResult.NewValue))
                        {
                            logFile.WriteLine("Matched DOI: " + doi + " -> " + regexMatchResult.NewValue);
                        }

                        additionalCrossRefDOISet.Add(regexMatchResult.NewValue);
                    }
                });
            });
            var crossrefFolderInfo = CrossRefCacheBuilder.SearchCrossRefFolder(dataFolderPath + "/external");
            DataProcessor.DataCiteFoundDOICache.Update(additionalCrossRefDOISet.ToList(), dataFolderPath, crossrefFolderInfo.FullName);
            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : End");
            logFile.Close();



        }
        */

    }
}
