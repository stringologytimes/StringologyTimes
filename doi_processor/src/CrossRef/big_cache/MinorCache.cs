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


            var dic = new Dictionary<string, string>();
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
            System.Threading.Tasks.Parallel.For(0, tsvFiles.Length, options, i =>
            {
                var tsvFile = tsvFiles[i];
                var lines = File.ReadAllLines(tsvFile);
                foreach (var line in lines)
                {
                    var cols = line.Split("\t");
                    if (cols.Length >= 3)
                    {
                        var doi = cols[0];
                        var type = cols[1];
                        var title = cols[2];

                        if (checkTypeHashSet.Contains(type))
                        {
                            lock (lockObj)
                            {


                                for (int j = 3; j < cols.Length; j++)
                                {
                                    var ISBN = cols[j];                                    
                                    if (ISBN.Length > 0)
                                    {
                                        var isValid = ISBNConverter.IsValidIsbn10(ISBN);
                                        if (isValid)
                                        {
                                            var isbn13 = ISBNConverter.Isbn10ToIsbn13(ISBN);
                                            ISBN = isbn13;
                                        }

                                        dic[ISBN] = doi;

                                        var firstChar = ISBN[0];

                                        bool xb = int.TryParse(firstChar.ToString(), out int result);
                                        if (!xb)
                                        {
                                            Console.WriteLine("ISBN: " + ISBN + " / " + firstChar.ToString());
                                            Console.WriteLine(line);
                                        }
                                    }
                                    //typeHashSet.Add(type);
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

            });
            CSVFunctions.WriteCSVAsDictionary(isbnFilePath, dic);
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


            var dic = new Dictionary<string, string>();
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
            System.Threading.Tasks.Parallel.For(0, tsvFiles.Length, options, i =>
            {
                var tsvFile = tsvFiles[i];
                var lines = File.ReadAllLines(tsvFile);
                foreach (var line in lines)
                {
                    var cols = line.Split("\t");
                    if (cols.Length >= 3)
                    {
                        var containerTitle = cols[2];
                        var doi = cols[0];
                        var type = cols[1];
                        var title = cols[2];
                        var isbnList = cols.Skip(3).ToList();
                        //bool b = isbnList.Any(isbn => isbn.Length > 0);

                        if (checkTypeHashSet.Contains(type) && title.Length > 0)
                        {
                            lock (lockObj)
                            {
                                dic[title] = doi;
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



            /*
                        foreach (var type in typeHashSet)
                        {
                            Console.WriteLine("Type: " + type);
                        }
                        */

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

            var typeHashSet = new HashSet<string>();

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
                    if (cols.Length >= 3)
                    {
                        var type = cols[1];
                        if (!typeHashSet.Contains(type))
                        {
                            lock (lockObj)
                            {
                                typeHashSet.Add(type);
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
            CSVFunctions.WriteCSV(typeListFilePath, typeHashSet.ToList());
        }


    }
}