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

namespace DataProcessor
{
    class DataCitePreprocessor
    {



        public static void CreateGZFileToDOIFolder(string dataCiteFolderPath, string dataFolderPath)
        {
            Console.WriteLine("Creating DOI List(DataCite): ");

            var main_folder = new DirectoryInfo(dataFolderPath + "/auto_generated/cache/datacite_cache");
            if (!main_folder.Exists)
            {
                main_folder.Create();
                Console.WriteLine("Created: " + main_folder.FullName);
            }

            var main_folder2 = new DirectoryInfo(dataFolderPath + "/auto_generated/cache/datacite_cache/gzfile_to_doi");
            if (!main_folder2.Exists)
            {
                main_folder2.Create();
                Console.WriteLine("Created: " + main_folder2.FullName);
            }


            //int maxCounter = 0;


            // gzファイル毎の処理を並列化し、各dict.Countを配列に格納
            var gzFiles = System.IO.Directory.GetFiles(dataCiteFolderPath, "*.jsonl.gz", System.IO.SearchOption.AllDirectories);

            var FinishedCounter = 0;
            var parallelCounter = 0;
            var skippedCounter = 0;
            object lockObj = new object();

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 32 // 最大並列度を4に制限
            };

            System.Threading.Tasks.Parallel.For(0, gzFiles.Length, options, i =>
            {
                var gzFilePath = gzFiles[i];
                FileInfo fi = new FileInfo(gzFilePath);
                lock (lockObj)
                {
                    parallelCounter++;
                }



                var name = (fi.Directory!.Name) + "_" + fi.Name;


                var csvFilePath = DataCiteJSONLLoader.GetGZFileToDOIFolderPath(dataFolderPath) + $"/{name}.csv";
                var csvFileInfo = new FileInfo(csvFilePath);


                if (!csvFileInfo.Exists)
                {
                    List<string> dois = new List<string>();

                    foreach (var line in JsonLib.ReadLinesFromGzip(gzFilePath))
                    {
                        var dict = JsonLib.CreateDictionaryFromJSONL(line);
                        if (dict.ContainsKey("id"))
                        {
                            dois.Add(dict["id"]);
                        }
                    }
                    var sw = new StreamWriter(csvFilePath, false, Encoding.UTF8);
                    foreach (var doi in dois)
                    {
                        sw.WriteLine(doi);
                    }
                    sw.Close();

                    lock (lockObj)
                    {
                        FinishedCounter++;
                        parallelCounter--;
                        if (FinishedCounter % 100 == 0)
                        {
                            Console.WriteLine("\t Processing: " + FinishedCounter + " / " + gzFiles.Length + " / Skipped: " + skippedCounter);
                        }
                    }


                }
                else
                {
                    lock (lockObj)
                    {
                        skippedCounter++;
                        FinishedCounter++;
                        parallelCounter--;
                    }
                }




            });

        }

        public static void CreateDOIToGZFileFolder(string doiListFolderPath, string dataFolderPath)
        {
            Console.WriteLine("Creating DOI Prefix to JSONL Map(DataCite): ");
            Dictionary<string, List<string>> doiPrefixToJSONLMap = new Dictionary<string, List<string>>();
            Dictionary<string, StreamWriter> onlineWriters = new Dictionary<string, StreamWriter>();
            HashSet<string> doiPrefixSet = new HashSet<string>();

            var main_folder = new DirectoryInfo(DataCiteJSONLLoader.GetDOIToGZFileFolderPath(dataFolderPath));
            if (!main_folder.Exists)
            {
                main_folder.Create();
            }

            // gzファイル毎の処理を並列化し、各dict.Countを配列に格納
            var csvFiles = System.IO.Directory.GetFiles(doiListFolderPath, "*.csv", System.IO.SearchOption.TopDirectoryOnly);
            //Dictionary<string, HashSet<string>> r = new Dictionary<string, HashSet<string>>();

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

                var dois = File.ReadAllLines(fi.FullName);
                lock (lockObj)
                {

                    foreach (var doi in dois)
                    {
                        var prefix = DOIFunctions.GetPrefix(doi);
                        doiPrefixSet.Add(prefix);

                        if (!doiPrefixToJSONLMap.ContainsKey(prefix))
                        {
                            doiPrefixToJSONLMap[prefix] = new List<string>();
                        }

                        if (onlineWriters.ContainsKey(prefix))
                        {
                            onlineWriters[prefix].WriteLine($"{doi},{directoryName},{gzFileName}");
                        }
                        else
                        {
                            doiPrefixToJSONLMap[prefix].Add($"{doi},{directoryName},{gzFileName}");

                            if (doiPrefixToJSONLMap[prefix].Count > 1000)
                            {
                                onlineWriters[prefix] = new StreamWriter(DataCiteJSONLLoader.GetDOIToGZFileFolderPath(dataFolderPath) + "/" + prefix + ".csv", false, Encoding.UTF8);
                                doiPrefixToJSONLMap[prefix].ForEach((v) =>
                                {
                                    onlineWriters[prefix].WriteLine(v);
                                });
                                doiPrefixToJSONLMap[prefix].Clear();
                            }

                        }
                    }
                    FinishedCounter++;
                    parallelCounter--;

                    if (FinishedCounter % 1000 == 0)
                    {
                        Console.WriteLine("\t Processing: " + FinishedCounter + " / " + csvFiles.Length + " / ");
                    }

                }

            });


            using (var sw = new StreamWriter(DataCiteJSONLLoader.GetDOIToGZFileFolderPath(dataFolderPath) + "/others.csv", false, Encoding.UTF8))
            {
                doiPrefixToJSONLMap.ToList().ForEach((v) =>
                {
                    v.Value.ForEach((w) =>
                    {
                        sw.WriteLine(w);
                    });
                });
                doiPrefixToJSONLMap.Clear();

            }

            using (var sw = new StreamWriter(DataCiteJSONLLoader.GetDOIToGZFileFolderPath(dataFolderPath) + "/doi_prefix.csv", false, Encoding.UTF8))
            {
                doiPrefixSet.ToList().ForEach((v) =>
                {
                    sw.WriteLine(v);
                });

            }


            onlineWriters.ToList().ForEach((v) =>
            {
                v.Value.Close();
            });
            onlineWriters.Clear();
        }

        private static void CreateFoundJSONLFileSub(List<string> dois, List<string> gzJSONLPaths, Dictionary<string, string> foundJSONLMap)
        {
            HashSet<string> doiSet = new HashSet<string>(dois);
            var maxCount = gzJSONLPaths.Count;
            var counter = 0;
            gzJSONLPaths.ForEach((v) =>
            {
                counter++;
                var fileInfo = new FileInfo(v);
                Console.WriteLine("\t\t Loading JSONL: " + fileInfo.Name + " / " + counter + " / " + maxCount);

                foreach (var line in JsonLib.ReadLinesFromGzip(v))
                {
                    var dict = JsonLib.CreateDictionaryFromJSONL(line);
                    var doi = dict["id"];
                    if (doiSet.Contains(doi))
                    {
                        foundJSONLMap[doi] = line;
                    }
                }

            });

        }

        public static void CreateFoundJSONLFile(List<string> dois, string dataFolderPath, string dataCiteFolderPath)
        {
            Console.WriteLine("Filtering JSONL(DataCite): ");
            var foundJSONLMapFilePath = dataFolderPath + "/auto_generated/cache/datacite_cache/found_jsonl.csv";
            Dictionary<string, string> foundJSONLMap = DataCiteJSONLLoader.Load(foundJSONLMapFilePath);
            Console.WriteLine("\t Found JSONL Map: " + foundJSONLMap.Count);

            Dictionary<string, HashSet<string>> doiPrefixToDoi = new Dictionary<string, HashSet<string>>();
            foreach (var doi in dois)
            {
                var smallDOI = doi.ToLower();
                if (!foundJSONLMap.ContainsKey(smallDOI))
                {
                    var doiPrefix = DOIFunctions.GetPrefix(smallDOI);
                    if (!doiPrefixToDoi.ContainsKey(doiPrefix))
                    {
                        doiPrefixToDoi[doiPrefix] = new HashSet<string>();
                    }
                    doiPrefixToDoi[doiPrefix].Add(smallDOI.ToLower());
                }
            }
            var doiPrefixMacCount = doiPrefixToDoi.Count;
            var doiPrefixCounter = 0;
            var gzFilePathSet = new HashSet<string>();
            var othersHashSet = new HashSet<string>();

            foreach (var kvp in doiPrefixToDoi)
            {
                doiPrefixCounter++;

                var doiPrefix = kvp.Key;
                var filePath = $"{DataCiteJSONLLoader.GetDOIToGZFileFolderPath(dataFolderPath)}/{doiPrefix}.csv";
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Exists)
                {
                    Console.Write("\r\t\t Processing DOI Prefix [" + doiPrefixCounter + " / " + doiPrefixMacCount + "]");

                    //Console.WriteLine("\t Processing File: " + fileInfo.Name + "/" + doiPrefix);
                    var lines = File.ReadAllLines(filePath);

                    foreach (var line in lines)
                    {
                        var cols = line.Split(",");
                        if (cols.Length == 3)
                        {
                            var lineDOI = cols[0];
                            var directoryName = cols[1];
                            var fileName = cols[2];
                            var gzFileName = directoryName + "/" + fileName;
                            if (kvp.Value.Contains(lineDOI))
                            {
                                gzFilePathSet.Add(dataCiteFolderPath + "/dois/" + gzFileName);
                            }
                        }
                    }
                }
                else
                {
                    kvp.Value.ToList().ForEach((v) =>
                    {
                        othersHashSet.Add(v);
                    });
                }

            }
            Console.WriteLine();
            Console.WriteLine("\t Found JSONL Map: " + foundJSONLMap.Count);

            var othersFilePath = $"{DataCiteJSONLLoader.GetDOIToGZFileFolderPath(dataFolderPath)}/others.csv";
            var othersFileInfo = new FileInfo(othersFilePath);
            if (othersFileInfo.Exists)
            {
                var lines = File.ReadAllLines(othersFilePath);
                foreach (var line in lines)
                {
                    var cols = line.Split(",");
                    if (cols.Length == 3)
                    {
                        var lineDOI = cols[0];
                        var directoryName = cols[1];
                        var fileName = cols[2];
                        var gzFileName = directoryName + "/" + fileName;
                        if (othersHashSet.Contains(lineDOI))
                        {
                            gzFilePathSet.Add(dataCiteFolderPath + "/dois/" + gzFileName);
                        }
                    }
                }
            }
            List<string> gzJSONLPaths = gzFilePathSet.ToList();
            CreateFoundJSONLFileSub(dois, gzJSONLPaths, foundJSONLMap);

            using (var JSONLCacheWriter = new StreamWriter(dataFolderPath + "/auto_generated/cache/datacite_cache/found_jsonl.csv", false, Encoding.UTF8))
            {
                foreach (var jsonl in foundJSONLMap.Values)
                {
                    JSONLCacheWriter.WriteLine(jsonl);
                }
            }

        }

        public static void PreprocessAll(string dataFolderPath, Dictionary<string, List<string>> doiToTagMapper)
        {
            var dataCiteDoiListFolderPath = dataFolderPath + "/auto_generated/cache/datacite_cache/gzfile_to_doi";

            var dataCiteFolderInfo = DataCiteJSONLLoader.SearchDataCiteFolder(dataFolderPath + "/external");

            DataProcessor.DataCitePreprocessor.CreateGZFileToDOIFolder(dataCiteFolderInfo.FullName, dataFolderPath);
            var dataCiteOtherCSVPath = dataFolderPath + "/auto_generated/cache/datacite_cache/doi_to_gzfile/others.csv";
            var dataCiteOtherCSVFileInfo = new FileInfo(dataCiteOtherCSVPath);
            if (!dataCiteOtherCSVFileInfo.Exists)
            {
                DataProcessor.DataCitePreprocessor.CreateDOIToGZFileFolder(dataCiteDoiListFolderPath, dataFolderPath);
            }

            DataProcessor.DataCitePreprocessor.CreateFoundJSONLFile(doiToTagMapper.Keys.ToList(), dataFolderPath, dataCiteFolderInfo.FullName);


        }
    }
}
