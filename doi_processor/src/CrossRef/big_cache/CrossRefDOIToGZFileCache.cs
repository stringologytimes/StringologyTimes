using System.Text;
using System.IO.Compression;
using System.Collections.ObjectModel;


namespace DataProcessor
{
    class CrossRefDOIToGZFileCache
    {
        public static string GetOthersFilePath(string dataFolderPath)
        {
            return GetDOIToGZFileFolderPath(dataFolderPath) + "/others.tsv";
        }
        public static string GetDOIPrefixFilePath(string dataFolderPath)
        {
            return GetDOIToGZFileFolderPath(dataFolderPath) + "/doi_prefix.tsv";
        }
        public static string GetDOIToGZFileFolderPath(string dataFolderPath)
        {
            return dataFolderPath + "/auto_generated/cache/crossref_cache/big_cache/doi_to_gzfile";
        }
        public static string GetISBNFilePath(string dataFolderPath)
        {
            return dataFolderPath + "/auto_generated/cache/crossref_cache/big_cache/isbn.tsv";
        }
        public static string GetTitleFilePath(string dataFolderPath)
        {
            return dataFolderPath + "/auto_generated/cache/crossref_cache/big_cache/title.tsv";
        }

        public static HashSet<string> GetDOIPrefixSet(string dataFolderPath)
        {
            var path = GetDOIPrefixFilePath(dataFolderPath);
            var fileInfo = new FileInfo(path);
            HashSet<string> doiPrefixSet = new HashSet<string>();
            if (fileInfo.Exists)
            {
                var lines = File.ReadAllLines(path);
                foreach (var line in lines)
                {
                    doiPrefixSet.Add(line);
                }
            }
            return doiPrefixSet;
        }

        public static void Build(string dataFolderPath)
        {
            Console.WriteLine("Creating DOI Prefix to JSONL Map(CrossRef): ");
            var doiListFolderPath = GetDOIToGZFileFolderPath(dataFolderPath);
            Dictionary<string, List<string>> doiPrefixToJSONLMap = new Dictionary<string, List<string>>();
            Dictionary<string, StreamWriter> onlineWriters = new Dictionary<string, StreamWriter>();
            HashSet<string> doiPrefixSet = new HashSet<string>();

            var main_folder = new DirectoryInfo(GetDOIToGZFileFolderPath(dataFolderPath));
            if (!main_folder.Exists)
            {
                main_folder.Create();
            }

            // gzファイル毎の処理を並列化し、各dict.Countを配列に格納
            var tsvFiles = System.IO.Directory.GetFiles(doiListFolderPath, "*.tsv", System.IO.SearchOption.TopDirectoryOnly);
            //Dictionary<string, HashSet<string>> r = new Dictionary<string, HashSet<string>>();

            var FinishedCounter = 0;
            var parallelCounter = 0;
            object lockObj = new object();

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 8 // 最大並列度を4に制限
            };

            System.Threading.Tasks.Parallel.For(0, tsvFiles.Length, options, i =>
            {
                lock (lockObj)
                {
                    Console.WriteLine("Processing: " + i + " / " + tsvFiles.Length);
                }

                var tsvFilePath = tsvFiles[i];
                FileInfo fi = new FileInfo(tsvFilePath);
                lock (lockObj)
                {
                    parallelCounter++;

                }
                var gzFileName = System.IO.Path.GetFileNameWithoutExtension(fi.Name);
                var doi_and_types = File.ReadAllLines(fi.FullName);
                lock (lockObj)
                {

                    foreach (var doi_and_type_line in doi_and_types)
                    {
                        var cols = doi_and_type_line.Split("\t");
                        if (cols.Length < 2)
                        {
                            Console.WriteLine("Error: " + doi_and_type_line + " / " + fi.FullName);
                            continue;
                        }
                        var doi = cols[0];
                        var type = cols[1];
                        var prefix = DOIFunctions.GetPrefix(doi);
                        doiPrefixSet.Add(prefix);
                        if (!doiPrefixToJSONLMap.ContainsKey(prefix))
                        {
                            doiPrefixToJSONLMap[prefix] = new List<string>();
                        }

                        if (onlineWriters.ContainsKey(prefix))
                        {
                            onlineWriters[prefix].WriteLine($"{doi}\t{gzFileName}");
                        }
                        else
                        {
                            doiPrefixToJSONLMap[prefix].Add($"{doi}\t{gzFileName}");

                            if (doiPrefixToJSONLMap[prefix].Count > 1000)
                            {
                                onlineWriters[prefix] = new StreamWriter(GetDOIToGZFileFolderPath(dataFolderPath) + "/" + prefix + ".tsv", false, Encoding.UTF8);
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
                        Console.WriteLine("\t Processing: " + FinishedCounter + " / " + tsvFiles.Length + " / ");
                    }

                }

            });

            using (var sw = new StreamWriter(GetOthersFilePath(dataFolderPath), false, Encoding.UTF8))
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

            using (var sw = new StreamWriter(GetDOIPrefixFilePath(dataFolderPath), false, Encoding.UTF8))
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

/*
        public static Dictionary<string, string> BuildDictionaryFromContainerTitleToDOI(string doiListFolderPath)
        {
            Console.WriteLine("Building Dictionary From Container Title to DOI(CrossRef): ");
            var dic = new Dictionary<string, string>();
            var tsvFiles = System.IO.Directory.GetFiles(doiListFolderPath, "*.tsv", System.IO.SearchOption.TopDirectoryOnly);
            var lockObj = new object();

            var maxCount = tsvFiles.Length;
            var counter = 0;
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 32 // 最大並列度を4に制限
            };
            System.Threading.Tasks.Parallel.For(0, tsvFiles.Length, options, i =>
            {
                var tsvFile = tsvFiles[i];
                var lines = File.ReadAllLines(tsvFile);
                foreach (var line in lines)
                {
                    var cols = line.Split("\t");
                    if (cols.Length == 3)
                    {
                        var containerTitle = cols[2];
                        var doi = cols[0];
                        if (containerTitle.Length > 0)
                        {
                            lock (lockObj)
                            {
                                dic[containerTitle] = doi;
                            }
                        }
                    }
                }

                lock (lockObj)
                {
                    counter++;
                    if (counter % 1000 == 0)
                    {
                        Console.WriteLine($"Processing: {counter} / {maxCount}");
                    }

                }

            });
            return dic;
        }
        */




    }
}
