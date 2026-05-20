using System.Text;
using System.IO.Compression;
using System.Collections.ObjectModel;


namespace DataProcessor
{
    class DataCiteGZFileToDOICache
    {
        class DOIElement1
        {
            public string DOI { get; set; } = "";
            public string Type { get; set; } = "";
            public string Title { get; set; } = "";
        }
        public static string GetFolderPath(string dataFolderPath)
        {
            return dataFolderPath + "/auto_generated/cache/datacite_cache/gzfile_to_doi";
        }

        public static void Build(string dataCiteFolderPath, string dataFolderPath)
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


                var csvFilePath = GetFolderPath(dataFolderPath) + $"/{name}.tsv";
                var csvFileInfo = new FileInfo(csvFilePath);


                if (!csvFileInfo.Exists)
                {
                    //List<KeyValuePair<string, string>> dois = new List<KeyValuePair<string, string>>();
                    var sw = new StreamWriter(csvFilePath, false, Encoding.UTF8);

                    foreach (var line in JsonLib.ReadLinesFromGzip(gzFilePath))
                    {
                        //var element = new DOIElement1();
                        var dict = JsonLib.CreateDictionaryFromJSONL(line);
                        if (dict.ContainsKey("id"))
                        {
                            var doi = dict["id"];
                            var type = DOIElement.GetTypeFromDataCiteJSONL(line);
                            var title = DOIElement.GetTitleFromDataCiteJSONL(line);
                            if (title == null)
                            {
                                throw new Exception("Title is null: " + line);
                            }
                            title = CSVFunctions.DeleteNewLineCode(title);
                            sw.WriteLine(doi + "\t" + type + "\t" + title);
                        }
                    }
                    sw.Close();

                    /*
                    foreach (var doi in dois)
                    {
                        sw.WriteLine(doi.Key + "\t" + doi.Value);
                    }
                    */

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

    }
}