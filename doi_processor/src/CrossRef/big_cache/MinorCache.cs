using System.Text;
using System.IO.Compression;
using System.Collections.ObjectModel;


namespace DataProcessor
{
    class MinorCache
    {
        public static string GetTypeListFilePath(string dataFolderPath)
        {
            return dataFolderPath + "/auto_generated/cache/crossref_cache/big_cache/type_list.tsv";
        }

        public static HashSet<string> GetCheckTypeHashSet(string dataFolderPath)
        {
            var checkTypeHashSet = new HashSet<string>();
            checkTypeHashSet.Add("book");
            checkTypeHashSet.Add("edited-book");
            checkTypeHashSet.Add("journal");
            checkTypeHashSet.Add("proceedings");
            checkTypeHashSet.Add("journal-volume");
            checkTypeHashSet.Add("book-series");
            checkTypeHashSet.Add("proceedings-series");
            checkTypeHashSet.Add("monograph");
            checkTypeHashSet.Add("reference-book");
            return checkTypeHashSet;
        }

        public static void BuildISBNFile(string dataFolderPath)
        {
            Console.WriteLine("Building Dictionary From ISBN to DOI(CrossRef): ");
            var doiListFolderPath = CrossRefGZFileToDOICache.GetGZFileToDoiFolderPath(dataFolderPath);

            var isbnFilePath = CrossRefDOIToGZFileCache.GetISBNFilePath(dataFolderPath);
            var isbnFile = new FileInfo(isbnFilePath);
            if (isbnFile.Exists)
            {
                Console.WriteLine("ISBN File already exists: " + isbnFilePath);
                return;
            }

            var tsvFiles = System.IO.Directory.GetFiles(doiListFolderPath, "*.tsv", System.IO.SearchOption.TopDirectoryOnly);
            var lockObj = new object();

            //var typeHashSet = new HashSet<string>();

            var checkTypeHashSet = GetCheckTypeHashSet(dataFolderPath);

            var maxCount = tsvFiles.Length;
            var counter = 0;
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 32 // 最大並列度を4に制限
            };

            var results = tsvFiles.AsParallel().Select<string, List<KeyValuePair<string, string>>>(tsvFile =>
                            {
                                var lines = File.ReadAllLines(tsvFile);
                                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();


                                foreach (var line in lines)
                                {
                                    var element = DOIElement1.ParseFromTSVString(line);
                                    if (element != null)
                                    {
                                        if (checkTypeHashSet.Contains(element.Type))
                                        {
                                            foreach (var isStr in element.ISList)
                                            {
                                                if (isStr.StartsWith("ISBN:"))
                                                {
                                                    var isbn = isStr.Substring(5);
                                                    pairs.Add(new KeyValuePair<string, string>(isbn, element.DOI));
                                                }                                                
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

                                return pairs;
                            }).ToList();

            var dic = new Dictionary<string, string>();

            foreach (var resultList in results)
            {
                foreach (var pair in resultList)
                {
                    dic[pair.Key] = pair.Value;
                }
            }

            CSVFunctions.WriteCSVAsDictionary(isbnFilePath, dic);
        }

        public static void BuildISSNFile(string dataFolderPath)
        {
            Console.WriteLine("Building Dictionary From ISSN to DOI(CrossRef): ");
            var doiListFolderPath = CrossRefGZFileToDOICache.GetGZFileToDoiFolderPath(dataFolderPath);

            var issnFilePath = CrossRefDOIToGZFileCache.GetISSNFilePath(dataFolderPath);
            var issnFile = new FileInfo(issnFilePath);
            if (issnFile.Exists)
            {
                Console.WriteLine("ISSN File already exists: " + issnFilePath);
                return;
            }

            var tsvFiles = System.IO.Directory.GetFiles(doiListFolderPath, "*.tsv", System.IO.SearchOption.TopDirectoryOnly);
            var lockObj = new object();

            //var typeHashSet = new HashSet<string>();

            var checkTypeHashSet = GetCheckTypeHashSet(dataFolderPath);

            var maxCount = tsvFiles.Length;
            var counter = 0;
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 32 // 最大並列度を4に制限
            };

            var results = tsvFiles.AsParallel().Select<string, Dictionary<string, string>>(tsvFile =>
                            {
                                var lines = File.ReadAllLines(tsvFile);
                                var tmp_dic = new Dictionary<string, string>();


                                foreach (var line in lines)
                                {
                                    var element = DOIElement1.ParseFromTSVString(line);
                                    if (element != null)
                                    {
                                        if (checkTypeHashSet.Contains(element.Type))
                                        {
                                            foreach (var isStr in element.ISList)
                                            {
                                                if (isStr.StartsWith("ISSN:"))
                                                {
                                                    var issn = isStr.Substring(5);
                                                    tmp_dic[issn] = element.DOI;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            foreach (var isStr in element.ISList)
                                            {
                                                if (isStr.StartsWith("ISSN:"))
                                                {
                                                    var issn = isStr.Substring(5);
                                                    if (!tmp_dic.ContainsKey(issn))
                                                    {
                                                        tmp_dic[issn] = "DUMMY:" + element.DOI;
                                                    }
                                                }
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

                                return tmp_dic;
                            }).ToList();

            var dic = new Dictionary<string, string>();

            foreach (var resultList in results)
            {
                foreach (var keyValuePair in resultList)
                {
                    if (keyValuePair.Value.StartsWith("DUMMY:"))
                    {
                        if (!dic.ContainsKey(keyValuePair.Key))
                        {
                            dic[keyValuePair.Key] = keyValuePair.Value;
                        }
                    }
                    else
                    {
                        dic[keyValuePair.Key] = keyValuePair.Value;
                    }
                }
            }

            CSVFunctions.WriteCSVAsDictionary(issnFilePath, dic);
        }

        public static void BuildTitleFile(string dataFolderPath)
        {
            Console.WriteLine("Building Dictionary From Container Title to DOI(CrossRef): ");
            var doiListFolderPath = CrossRefGZFileToDOICache.GetGZFileToDoiFolderPath(dataFolderPath);

            var titleFilePath = CrossRefDOIToGZFileCache.GetTitleFilePath(dataFolderPath);
            var titleFile = new FileInfo(titleFilePath);
            if (titleFile.Exists)
            {
                Console.WriteLine("Title File already exists: " + titleFilePath);
                return;
            }


            string[] tsvFiles = System.IO.Directory.GetFiles(doiListFolderPath, "*.tsv", System.IO.SearchOption.TopDirectoryOnly);
            var lockObj = new object();

            //var typeHashSet = new HashSet<string>();

            var checkTypeHashSet = GetCheckTypeHashSet(dataFolderPath);

            var maxCount = tsvFiles.Length;
            var counter = 0;
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 32 // 最大並列度を4に制限
            };

            var results = tsvFiles.AsParallel().Select<string, List<KeyValuePair<string, string>>>(tsvFile =>
                {
                    var lines = File.ReadAllLines(tsvFile);
                    List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();


                    foreach (var line in lines)
                    {
                        var element = DOIElement1.ParseFromTSVString(line);
                        if (element != null)
                        {
                            if (checkTypeHashSet.Contains(element.Type) && element.Title.Length > 0)
                            {
                                pairs.Add(new KeyValuePair<string, string>(element.Title, element.DOI));
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

                    return pairs;
                }).ToList();

            var dic = new Dictionary<string, string>();

            foreach (var resultList in results)
            {
                foreach (var pair in resultList)
                {
                    dic[pair.Key] = pair.Value;
                }
            }

            CSVFunctions.WriteCSVAsDictionary(titleFilePath, dic);
        }

        public static void BuildTypeListFile(string dataFolderPath)
        {
            Console.WriteLine("Building Type List File: ");
            var doiListFolderPath = CrossRefGZFileToDOICache.GetGZFileToDoiFolderPath(dataFolderPath);

            var typeListFilePath = GetTypeListFilePath(dataFolderPath);
            var typeListFile = new FileInfo(typeListFilePath);
            if (typeListFile.Exists)
            {
                Console.WriteLine("Type List File already exists: " + typeListFilePath);
                return;
            }


            var dic = new Dictionary<string, string>();
            var tsvFiles = System.IO.Directory.GetFiles(doiListFolderPath, "*.tsv", System.IO.SearchOption.TopDirectoryOnly);
            var lockObj = new object();

            //var typeHashSet = new HashSet<string>();


            var maxCount = tsvFiles.Length;
            var counter = 0;
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 32 // 最大並列度を4に制限
            };
            var results = tsvFiles.AsParallel().Select<string, HashSet<string>>(tsvFile =>
                            {
                                var lines = File.ReadAllLines(tsvFile);
                                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();

                                var tmpHashSet = new HashSet<string>();


                                foreach (var line in lines)
                                {
                                    var element = DOIElement1.ParseFromTSVString(line);
                                    if (element != null)
                                    {
                                        tmpHashSet.Add(element.Type);
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

                                return tmpHashSet;
                            }).ToList();
            var typeHashSet = new HashSet<string>();


            foreach (var resultList in results)
            {
                foreach (var type in resultList)
                {
                    typeHashSet.Add(type);
                }
            }
            CSVFunctions.WriteCSV(typeListFilePath, typeHashSet.ToList());
        }


    }
}