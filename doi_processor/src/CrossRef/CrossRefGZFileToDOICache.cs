using System.Text;
using System.IO.Compression;
using System.Collections.ObjectModel;
using System.Security.Cryptography;

namespace DataProcessor
{
    class CrossRefGZFileToDOICache
    {
        public static void Build(string jsonlFolderPath, string dataFolderPath)
        {
            Console.WriteLine("Creating DOI List(CrossRef): ");

            var main_folder = new DirectoryInfo(CrossRefCacheBuilder.GetGZFileToDoiFolderPath(dataFolderPath));
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
                string parentName = fi.Directory!.Name;
                string fileName = fi.Name;

                lock (lockObj)
                {
                    parallelCounter++;
                }

            
                var csvFilePath = CrossRefCacheBuilder.GetGZFileToDoiFolderPath(dataFolderPath) + $"/{fi.Name}.csv";
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
                        if (FinishedCounter % 1000 == 0)
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
                        if (FinishedCounter % 1000 == 0)
                        {
                            Console.WriteLine("\t Processing: " + FinishedCounter + " / " + gzFiles.Length + " / Skipped: " + skippedCounter);
                        }

                    }
                }



            });




        }
    }
}
