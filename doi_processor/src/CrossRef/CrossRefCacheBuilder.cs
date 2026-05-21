using System.Text;
using System.IO.Compression;
using System.Collections.ObjectModel;


namespace DataProcessor
{
    class CrossRefPreprocessor
    {
        public static DirectoryInfo SearchCrossRefFolder(string externalFolderPath)
        {
            DirectoryInfo di = new DirectoryInfo(externalFolderPath);
            if (di.Exists)
            {
                string CrossRefFilePath = "";

                foreach (var dir in Directory.GetDirectories(di.FullName)) // 直下のフォルダのみ
                {
                    var containsGZ = false;
                    var nameCheck = dir.IndexOf("Crossref") != -1;


                    foreach (var file in Directory.EnumerateFiles(dir)) // 直下のファイルのみ
                    {
                        FileInfo fi = new FileInfo(file);

                        if (fi.Extension == ".gz")
                        {
                            containsGZ = true;
                        }
                    }
                    if (containsGZ && nameCheck)
                    {
                        CrossRefFilePath = dir;
                    }
                }
                if (CrossRefFilePath != "")
                {
                    return new DirectoryInfo(CrossRefFilePath);
                }
            }
            throw new Exception("CrossRef folder not found");
        }

        public static void BuildBigCache(string dataFolderPath)
        {
            var crossrefFolderInfo = CrossRefPreprocessor.SearchCrossRefFolder(dataFolderPath + "/external");

            DataProcessor.CrossRefGZFileToDOICache.Build(dataFolderPath);

            var otherCSVPath = CrossRefDOIToGZFileCache.GetOthersFilePath(dataFolderPath);
            var otherCSVFileInfo = new FileInfo(otherCSVPath);
            if (!otherCSVFileInfo.Exists)
            {
                CrossRefDOIToGZFileCache.Build(dataFolderPath);
            }

            //BuildBookCache(dataFolderPath, crossRefDoiListFolderPath);

        }




        public static async Task UpdateSmallCache(string dataFolderPath, HashSet<string> doiSet, string mailAddress)
        {
            Console.WriteLine("Building CrossRefSmallCache [START]");
            var crossrefFolderInfo = CrossRefPreprocessor.SearchCrossRefFolder(dataFolderPath + "/external");

            var otherCSVPath = CrossRefDOIToGZFileCache.GetOthersFilePath(dataFolderPath);
            var otherCSVFileInfo = new FileInfo(otherCSVPath);
            if (!otherCSVFileInfo.Exists)
            {
                throw new Exception("others.tsv not found");
            }

            // Build Found DOI Cache
            CrossRefFoundDOICache.Update(doiSet.ToList(), dataFolderPath, crossrefFolderInfo.FullName);

            
            var unknownDOIFilePath = CrossRefExternalFoundDOICache.GetUnknownDOIFilePath(dataFolderPath);
            var unknownDOISet = CSVFunctions.ReadCSVAsHashSet(unknownDOIFilePath);
            await CrossRefExternalFoundDOICache.Build(dataFolderPath, doiSet, unknownDOISet, mailAddress);
            CSVFunctions.WriteCSV(unknownDOIFilePath, unknownDOISet);


            Console.WriteLine("CrossRefSmallCache [END]");

        }

    }
}
