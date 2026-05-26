using System.Text;
using System.IO.Compression;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text.Json;
namespace DataProcessor
{
    class CrossRefGZFileToDOICache
    {
        class DOIElement1
        {
            public string DOI { get; set; } = "";
            public string Type { get; set; } = "";
            public string Title { get; set; } = "";
            public List<string> ISBNList { get; set; } = new List<string>();
        }
        public static string GetGZFileToDoiFolderPath(string dataFolderPath)
        {
            return dataFolderPath + "/auto_generated/cache/crossref_cache/big_cache/gzfile_to_doi";
        }


        public static void Build(string dataFolderPath, string externalFolderPath)
        {
            Console.WriteLine("Creating DOI List(CrossRef): ");


            var main_folder = new DirectoryInfo(CrossRefGZFileToDOICache.GetGZFileToDoiFolderPath(dataFolderPath));
            if (!main_folder.Exists)
            {
                main_folder.Create();
            }


            // gzファイル毎の処理を並列化し、各dict.Countを配列に格納
            var gzFiles = System.IO.Directory.GetFiles(externalFolderPath, "*.gz", System.IO.SearchOption.TopDirectoryOnly);

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


                var csvFilePath = CrossRefGZFileToDOICache.GetGZFileToDoiFolderPath(dataFolderPath) + $"/{fi.Name}.tsv";
                var csvFileInfo = new FileInfo(csvFilePath);

                if (!csvFileInfo.Exists)
                {
                    List<DOIElement1> dois = new List<DOIElement1>();

                    foreach (var line in JsonLib.ReadLinesFromGzip(gzFilePath))
                    {
                        var dict = JsonLib.CreateDictionaryFromJSONL(line);
                        if (dict.ContainsKey("DOI"))
                        {
                            var element = new DOIElement1();
                            element.DOI = dict["DOI"];
                            element.Type = "unknown";
                            if (dict.ContainsKey("type"))
                            {
                                element.Type = dict["type"];
                            }
                            element.Title = "";
                            if (dict.ContainsKey("title"))
                            {
                                var titleList = JsonSerializer.Deserialize<List<string>>(dict["title"]);
                                if (titleList != null && titleList.Count > 0)
                                {
                                    element.Title = CSVFunctions.DeleteNewLineCode(titleList[0]);
                                }
                            }

                            if (dict.ContainsKey("ISBN"))
                            {
                                var ISBNList = JsonSerializer.Deserialize<List<string>>(dict["ISBN"]);
                                if (ISBNList != null && ISBNList.Count > 0)
                                {
                                    element.ISBNList = ISBNList.ToList();
                                }
                            }


                            dois.Add(element);
                        }
                    }
                    var sw = new StreamWriter(csvFilePath, false, Encoding.UTF8);
                    foreach (var doi in dois)
                    {
                        if (doi.ISBNList.Count > 0)
                        {
                            sw.WriteLine(doi.DOI + "\t" + doi.Type + "\t" + doi.Title + "\t" + string.Join("\t", doi.ISBNList));
                        }
                        else
                        {
                            sw.WriteLine(doi.DOI + "\t" + doi.Type + "\t" + doi.Title);
                        }
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
