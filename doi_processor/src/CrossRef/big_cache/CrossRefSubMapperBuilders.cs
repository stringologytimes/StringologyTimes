using System.Text;
using System.IO.Compression;
using System.Collections.ObjectModel;


namespace DataProcessor
{
    class CrossRefSubMapperBuilders
    {
        public static Dictionary<string, string> LoadISBNMapper(string dataFolderPath)
        {
            var isbnFilePath = CrossRefDOIToGZFileCache.GetISBNFilePath(dataFolderPath);
            var dic = CSVFunctions.ReadCSVAasDictionary(isbnFilePath);
            return dic;
        }

        public static Dictionary<string, string> LoadISSNMapper(string dataFolderPath)
        {
            var issnFilePath = CrossRefDOIToGZFileCache.GetISSNFilePath(dataFolderPath);
            var dic = CSVFunctions.ReadCSVAasDictionary(issnFilePath);
            return dic;
        }
        public static Dictionary<string, List<string>> LoadTitleMapper(string dataFolderPath)
        {
            var titleFilePath = CrossRefDOIToGZFileCache.GetTitleFilePath(dataFolderPath);
            var dic = CSVFunctions.ReadCSVAasMultiDictionary(titleFilePath);
            return dic;
        }

        public static bool CheckType(string type)
        {
            switch (type)
            {
                case "book":
                case "edited-book":
                case "journal":
                case "proceedings":
                case "journal-volume":
                case "book-series":
                case "proceedings-series":
                case "monograph":
                case "reference-book":
                    return true;
                default:
                    return false;
            }
            
        }

        public static string GetTypeListFilePath(string dataFolderPath)
        {
            return dataFolderPath + "/auto_generated/cache/crossref_cache/big_cache/type_list.tsv";
        }
        private static void BuildMapperByProcessingDatasetTemplate(string dataFolderPath, string keyName, string outputFilePath)
        {
            Console.WriteLine("Building Dictionary From " + keyName + " to DOI(CrossRef): ");
            var doiListFolderPath = CrossRefGZFileToDOICache.GetGZFileToDoiFolderPath(dataFolderPath);

            var outputFile = new FileInfo(outputFilePath);
            if (outputFile.Exists)
            {
                Console.WriteLine("Output File already exists: " + outputFilePath);
                return;
            }

            var tsvFiles = System.IO.Directory.GetFiles(doiListFolderPath, "*.tsv", System.IO.SearchOption.TopDirectoryOnly);
            var lockObj = new object();

            //var typeHashSet = new HashSet<string>();

            //var checkTypeHashSet = MinorCache.GetCheckTypeHashSet(dataFolderPath);

            var maxCount = tsvFiles.Length;
            var counter = 0;
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 32 // 最大並列度を4に制限
            };

            var results = tsvFiles.AsParallel().Select<string, List<Tuple<string, string, string>>>(tsvFile =>
                            {
                                var lines = File.ReadAllLines(tsvFile);
                                List<Tuple<string, string, string>> pairs = new List<Tuple<string, string, string>>();


                                foreach (var line in lines)
                                {
                                    var element = DOIElementX.ParseFromTSVString(line);
                                    if (element != null)
                                    {
                                        if (CheckType(element.Type))
                                        {
                                            if (keyName == "ISBN")
                                            {
                                                foreach (var isStr in element.ISList)
                                                {
                                                    if (isStr.StartsWith("ISBN:"))
                                                    {
                                                        var isbn = ISBNConverter.ParseISBN(isStr.Substring(5));
                                                        pairs.Add(new Tuple<string, string, string>(isbn, element.DOI, element.Type));
                                                    }
                                                }
                                            }
                                            else if (keyName == "ISSN")
                                            {
                                                foreach (var isStr in element.ISList)
                                                {
                                                    if (isStr.StartsWith("ISSN:"))
                                                    {
                                                        var issn = ISBNConverter.ParseISSN(isStr.Substring(5));
                                                        pairs.Add(new Tuple<string, string, string>(issn, element.DOI, element.Type));
                                                    }
                                                }
                                            }
                                            else if (keyName == "Title")
                                            {

                                                if (element.Title.Length > 0)
                                                {
                                                    
                                                    pairs.Add(new Tuple<string, string, string>(element.Title, element.DOI, element.Type));
                                                }
                                            }
                                            else
                                            {
                                                throw new Exception("Invalid key name: " + keyName);
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

            Console.WriteLine("Constructing Table...");

            var table = new List<List<string>>();

            foreach (var resultList in results)
            {
                foreach (var pair in resultList)
                {
                    var row = new List<string>();
                    row.Add(pair.Item1);
                    row.Add(pair.Item2);
                    row.Add(pair.Item3);
                    table.Add(row);
                }
            }
            table.Sort((a, b) => a[0].CompareTo(b[0]));

            Console.WriteLine("Writing Table to File...");
            CSVFunctions.WriteCSV(outputFilePath, table);
            Console.WriteLine("Done");
        }


        public static void BuildISBNMapper(string dataFolderPath)
        {
            BuildMapperByProcessingDatasetTemplate(dataFolderPath, "ISBN", CrossRefDOIToGZFileCache.GetISBNFilePath(dataFolderPath));
        }

        public static void BuildISSNMapper(string dataFolderPath)
        {
            BuildMapperByProcessingDatasetTemplate(dataFolderPath, "ISSN", CrossRefDOIToGZFileCache.GetISSNFilePath(dataFolderPath));
        }

        public static void BuildTitleMapper(string dataFolderPath)
        {
            BuildMapperByProcessingDatasetTemplate(dataFolderPath, "Title", CrossRefDOIToGZFileCache.GetTitleFilePath(dataFolderPath));
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
                                    var element = DOIElementX.ParseFromTSVString(line);
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