using System.Text;
using System.IO.Compression;
using System.Collections.ObjectModel;


namespace DataProcessor
{
    class CrossRefPreprocessor
    {

        public static void BuildBigCache(string dataFolderPath)
        {
            var crossrefFolderInfo = CrossRefCacheBuilder.SearchCrossRefFolder(dataFolderPath + "/external");
            var crossRefDoiListFolderPath = dataFolderPath + "/auto_generated/cache/crossref_cache/gzfile_to_doi";

            DataProcessor.CrossRefGZFileToDOICache.Build(crossrefFolderInfo.FullName, dataFolderPath);

            var otherCSVPath = CrossRefDOIToGZFileCache.GetOthersFilePath(dataFolderPath);
            var otherCSVFileInfo = new FileInfo(otherCSVPath);
            if (!otherCSVFileInfo.Exists)
            {
                CrossRefDOIToGZFileCache.Build(crossRefDoiListFolderPath, dataFolderPath);
            }

            //BuildBookCache(dataFolderPath, crossRefDoiListFolderPath);

        }

        /*

        private static void BuildBookCache(string dataFolderPath, string doiListFolderPath)
        {
            Console.WriteLine("Creating DOI Prefix to JSONL Map(CrossRef): ");
            Dictionary<string, List<string>> doiPrefixToJSONLMap = new Dictionary<string, List<string>>();
            Dictionary<string, StreamWriter> onlineWriters = new Dictionary<string, StreamWriter>();

            var boolCacheFileInfo = new FileInfo(dataFolderPath + "/auto_generated/cache/crossref_cache/book_cache.tsv");
            var sw = new StreamWriter(boolCacheFileInfo.FullName, false, Encoding.UTF8);


            var boolCache = CSVFunctions.ReadCSV(boolCacheFileInfo.FullName);

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
                var gzFileName = System.IO.Path.GetFileNameWithoutExtension(fi.Name);
                var doi_and_types = File.ReadAllLines(fi.FullName);
                lock (lockObj)
                {

                    foreach (var doi_and_type_line in doi_and_types)
                    {
                        var cols = doi_and_type_line.Split("\t");
                        var doi = cols[0];
                        var type = cols[1];

                        if (type == "book" || type == "journal" || type == "proceedings" || type == "journal-volume")
                        {
                            sw.WriteLine(doi_and_type_line);
                        }
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

            foreach (var type in hashSet)
            {
                Console.WriteLine(type);
            }
            sw.Close();
        }
        */



        public static async Task BuildSmallCache(string dataFolderPath, HashSet<string> doiSet, string mailAddress)
        {
            Console.WriteLine("Building CrossRefSmallCache [START]");





            var crossrefFolderInfo = CrossRefCacheBuilder.SearchCrossRefFolder(dataFolderPath + "/external");

            var otherCSVPath = CrossRefDOIToGZFileCache.GetOthersFilePath(dataFolderPath);
            var otherCSVFileInfo = new FileInfo(otherCSVPath);
            if (!otherCSVFileInfo.Exists)
            {
                throw new Exception("others.tsv not found");
            }

            // Build Found DOI Cache
            CrossRefFoundDOICache.Update(doiSet.ToList(), dataFolderPath, crossrefFolderInfo.FullName);


            var unknownDOISet = CSVFunctions.ReadCSVAsHashSet(dataFolderPath + "/auto_generated/cache/crossref_cache/unknown_doi.tsv");
            await CrossRefExternalFoundDOICache.Build(dataFolderPath, doiSet, unknownDOISet, mailAddress);
            CSVFunctions.WriteCSV(dataFolderPath + "/auto_generated/cache/crossref_cache/unknown_doi.tsv", unknownDOISet);






            Console.WriteLine("CrossRefSmallCache [END]");

        }

    }
}
