using System.Text;
using System.IO.Compression;

namespace DataProcessor
{
    class CrossRefPreprocessor
    {




        private static void CreateGZFileToDOIFolder(string jsonlFolderPath, string dataFolderPath)
        {
            Console.WriteLine("Creating DOI List(CrossRef): ");

            var main_folder = new DirectoryInfo(CrossRefJSONLLoader.GetGZFileToDoiFolderPath(dataFolderPath));
            if (!main_folder.Exists)
            {
                main_folder.Create();
            }


            // gzファイル毎の処理を並列化し、各dict.Countを配列に格納
            var gzFiles = System.IO.Directory.GetFiles(jsonlFolderPath, "*.gz", System.IO.SearchOption.TopDirectoryOnly);

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
                var csvFilePath = CrossRefJSONLLoader.GetGZFileToDoiFolderPath(dataFolderPath) + $"/{fi.Name}.csv";
                var csvFileInfo = new FileInfo(csvFilePath);

                if (!csvFileInfo.Exists)
                {
                    List<string> dois = new List<string>();

                    foreach (var line in JsonLib.ReadLinesFromGzip(gzFilePath))
                    {
                        var dict = JsonLib.CreateDictionaryFromJSONL(line);
                        if (dict.ContainsKey("DOI"))
                        {
                            dois.Add(dict["DOI"]);
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


        private static void CreateDOIToGZFileFolder(string doiListFolderPath, string dataFolderPath)
        {
            Console.WriteLine("Creating DOI Prefix to JSONL Map(CrossRef): ");
            Dictionary<string, List<string>> doiPrefixToJSONLMap = new Dictionary<string, List<string>>();
            Dictionary<string, StreamWriter> onlineWriters = new Dictionary<string, StreamWriter>();
            HashSet<string> doiPrefixSet = new HashSet<string>();

            var main_folder = new DirectoryInfo(CrossRefJSONLLoader.GetDOIToGZFileFolderPath(dataFolderPath));
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
                var gzFileName = System.IO.Path.GetFileNameWithoutExtension(fi.Name);
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
                            onlineWriters[prefix].WriteLine($"{doi},{gzFileName}");
                        }
                        else
                        {
                            doiPrefixToJSONLMap[prefix].Add($"{doi},{gzFileName}");

                            if (doiPrefixToJSONLMap[prefix].Count > 1000)
                            {
                                onlineWriters[prefix] = new StreamWriter(CrossRefJSONLLoader.GetDOIToGZFileFolderPath(dataFolderPath) + "/" + prefix + ".csv", false, Encoding.UTF8);
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

            using (var sw = new StreamWriter(CrossRefJSONLLoader.GetDOIToGZFileFolderPath(dataFolderPath) + "/others.csv", false, Encoding.UTF8))
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

            using (var sw = new StreamWriter(CrossRefJSONLLoader.GetDOIToGZFileFolderPath(dataFolderPath) + "/doi_prefix.csv", false, Encoding.UTF8))
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
                    var doi = dict["DOI"];
                    if (doiSet.Contains(doi))
                    {
                        foundJSONLMap[doi] = line;
                    }
                }

            });

        }


        private static void CreateFoundJSONLFile(List<string> dois, string dataFolderPath, string jsonlFolderPath, Dictionary<string, string> crossRefExternalDic)
        {
            Console.WriteLine("Creating Found JSONL File(CrossRef): ");
            var dicPath = dataFolderPath + "/auto_generated/cache/crossref_cache/found_jsonl.csv";
            Dictionary<string, string> foundJSONLMap = CrossRefJSONLLoader.Load(dicPath);
            var crsosRefDOIPrefixSet = CrossRefJSONLLoader.GetDOIPrefixSet(dataFolderPath);
            Console.WriteLine("\t Found JSONL Map: " + foundJSONLMap.Count);

            //List<string> foundJSONLList = new List<string>();
            Dictionary<string, HashSet<string>> doiPrefixToDoi = new Dictionary<string, HashSet<string>>();
            foreach (var doi in dois)
            {
                    var doiPrefix = DOIFunctions.GetPrefix(doi);
                if (!foundJSONLMap.ContainsKey(doi) && crsosRefDOIPrefixSet.Contains(doiPrefix) && !crossRefExternalDic.ContainsKey(doi))
                {
                    if (!doiPrefixToDoi.ContainsKey(doiPrefix))
                    {
                        doiPrefixToDoi[doiPrefix] = new HashSet<string>();
                    }
                    doiPrefixToDoi[doiPrefix].Add(doi);
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
                var filePath = $"{CrossRefJSONLLoader.GetDOIToGZFileFolderPath(dataFolderPath)}/{doiPrefix}.csv";
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Exists)
                {
                    Console.Write("\r\t\t Processing DOI Prefix [" + doiPrefixCounter + " / " + doiPrefixMacCount + "]");

                    var lines = File.ReadAllLines(filePath);
                    foreach (var line in lines)
                    {
                        var cols = line.Split(",");
                        if (cols.Length == 2)
                        {
                            var lineDOI = cols[0];
                            var gzFileName = cols[1];
                            if (kvp.Value.Contains(lineDOI))
                            {
                                gzFilePathSet.Add(jsonlFolderPath + "/" + gzFileName);
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
            var othersFilePath = $"{CrossRefJSONLLoader.GetDOIToGZFileFolderPath(dataFolderPath)}/others.csv";
            var othersFileInfo = new FileInfo(othersFilePath);
            if (othersFileInfo.Exists)
            {
                var lines = File.ReadAllLines(othersFilePath);
                foreach (var line in lines)
                {
                    var cols = line.Split(",");
                    if (cols.Length == 2)
                    {
                        var lineDOI = cols[0];
                        var gzFileName = cols[1];
                        if (othersHashSet.Contains(lineDOI))
                        {
                            gzFilePathSet.Add(jsonlFolderPath + "/" + gzFileName);
                        }
                    }
                }
            }


            List<string> gzJSONLPaths = gzFilePathSet.ToList();
            CreateFoundJSONLFileSub(dois, gzJSONLPaths, foundJSONLMap);


            var JSONLCacheWriter = new StreamWriter(dataFolderPath + "/auto_generated/cache/crossref_cache/found_jsonl.csv", false, Encoding.UTF8);
            foreach (var jsonl in foundJSONLMap.Values)
            {
                JSONLCacheWriter.WriteLine(jsonl);
            }
            JSONLCacheWriter.Close();
        }
        public static void PreprocessAll(string dataFolderPath, Dictionary<string, List<string>> doiToTagMapper, Dictionary<string, string> crossRefExternalDic)
        {
            var crossrefFolderInfo = CrossRefJSONLLoader.SearchCrossRefFolder(dataFolderPath + "/external");
            var crossRefDoiListFolderPath = dataFolderPath + "/auto_generated/cache/crossref_cache/gzfile_to_doi";
            //var crossRefResultFilePath = dataFolderPath + "/auto_generated/crossref_articles.jsonl";


            DataProcessor.CrossRefPreprocessor.CreateGZFileToDOIFolder(crossrefFolderInfo.FullName, dataFolderPath);

            var otherCSVPath = dataFolderPath + "/auto_generated/cache/crossref_cache/doi_to_gzfile/others.csv";
            var otherCSVFileInfo = new FileInfo(otherCSVPath);
            if (!otherCSVFileInfo.Exists)
            {
                DataProcessor.CrossRefPreprocessor.CreateDOIToGZFileFolder(crossRefDoiListFolderPath, dataFolderPath);
            }

            CreateFoundJSONLFile(doiToTagMapper.Keys.ToList(), dataFolderPath, crossrefFolderInfo.FullName, crossRefExternalDic);

        }

    }
}
