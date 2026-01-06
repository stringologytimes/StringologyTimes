using System.Text;
using System.IO.Compression;
using System.Collections.ObjectModel;


namespace DataProcessor
{
    class CrossRefDOIToGZFileCache
    {
        public static string GetDOIToGZFileFolderPath(string dataFolderPath)
        {
            return dataFolderPath + "/auto_generated/cache/crossref_cache/doi_to_gzfile";
        }

        public static HashSet<string> GetDOIPrefixSet(string dataFolderPath)
        {
            var path = GetDOIToGZFileFolderPath(dataFolderPath) + "/doi_prefix.csv";
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

        public static void Build(string doiListFolderPath, string dataFolderPath)
        {
            Console.WriteLine("Creating DOI Prefix to JSONL Map(CrossRef): ");
            Dictionary<string, List<string>> doiPrefixToJSONLMap = new Dictionary<string, List<string>>();
            Dictionary<string, StreamWriter> onlineWriters = new Dictionary<string, StreamWriter>();
            HashSet<string> doiPrefixSet = new HashSet<string>();

            var main_folder = new DirectoryInfo(GetDOIToGZFileFolderPath(dataFolderPath));
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
                                onlineWriters[prefix] = new StreamWriter(GetDOIToGZFileFolderPath(dataFolderPath) + "/" + prefix + ".csv", false, Encoding.UTF8);
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

            using (var sw = new StreamWriter(GetDOIToGZFileFolderPath(dataFolderPath) + "/others.csv", false, Encoding.UTF8))
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

            using (var sw = new StreamWriter(GetDOIToGZFileFolderPath(dataFolderPath) + "/doi_prefix.csv", false, Encoding.UTF8))
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
    }
}
