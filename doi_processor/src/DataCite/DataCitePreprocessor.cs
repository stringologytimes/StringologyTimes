using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace DataProcessor
{
    class DataCitePreprocessor
    {



        public static void BuildBigCache(string dataFolderPath)
        {
            var dataCiteDoiListFolderPath = dataFolderPath + "/auto_generated/cache/datacite_cache/gzfile_to_doi";

            var dataCiteFolderInfo = DataCiteJSONLLoader.SearchDataCiteFolder(dataFolderPath + "/external");

            DataProcessor.DataCiteGZFileToDOICache.Build(dataCiteFolderInfo.FullName, dataFolderPath);
            var dataCiteOtherCSVPath = DataCiteDOIToGZFileCache.GetOthersFilePath(dataFolderPath);
            var dataCiteOtherCSVFileInfo = new FileInfo(dataCiteOtherCSVPath);
            if (!dataCiteOtherCSVFileInfo.Exists)
            {
                DataCiteDOIToGZFileCache.Build(dataCiteDoiListFolderPath, dataFolderPath);
            }
            //BuildBookCache(dataCiteDoiListFolderPath, dataFolderPath);
        }





        public static async Task BuildSmallCache(string dataFolderPath, HashSet<string> doiSet, string mailAddress)
        {
            var dataCiteFolderInfo = DataCiteJSONLLoader.SearchDataCiteFolder(dataFolderPath + "/external");
            var dataCiteOtherCSVPath = DataCiteDOIToGZFileCache.GetOthersFilePath(dataFolderPath);

            var dataCiteOtherCSVFileInfo = new FileInfo(dataCiteOtherCSVPath);
            if (!dataCiteOtherCSVFileInfo.Exists)
            {
                throw new Exception("others.tsv not found");
            }

            DataCiteFoundDOICache.BuildUsingDOIToGZFileCache(doiSet.ToList(), dataFolderPath, dataCiteFolderInfo.FullName);

            var unknownDOISet = CSVFunctions.ReadCSVAsHashSet(dataFolderPath + "/auto_generated/cache/datacite_cache/unknown_doi.tsv");
            await DataCiteExternalFoundDOICache.Build(dataFolderPath, doiSet, unknownDOISet, mailAddress);
            CSVFunctions.WriteCSV(dataFolderPath + "/auto_generated/cache/datacite_cache/unknown_doi.tsv", unknownDOISet);





        }


        public static void BuildBookCache(string doiListFolderPath, string dataFolderPath)
        {
            Dictionary<string, StreamWriter> onlineWriters = new Dictionary<string, StreamWriter>();

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
                var csvFileName = System.IO.Path.GetFileNameWithoutExtension(fi.Name);
                var splits = csvFileName.Split("_");
                var directoryName = splits[0] + "_" + splits[1];
                var gzFileName = splits[2] + "_" + splits[3];

                var doi_and_types = File.ReadAllLines(fi.FullName);
                lock (lockObj)
                {

                    foreach (var doi_and_type_line in doi_and_types)
                    {
                        var cols = doi_and_type_line.Split("\t");
                        var doi = cols[0];
                        var type = cols[1];

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

            Console.WriteLine("DataCite Book Cache: " + hashSet.Count);
            
            foreach (var type in hashSet)
            {
                Console.WriteLine("DataCite: " + type);
            }


        }
    }
}
