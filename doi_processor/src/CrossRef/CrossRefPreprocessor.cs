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

            var otherCSVPath = dataFolderPath + "/auto_generated/cache/crossref_cache/doi_to_gzfile/others.csv";
            var otherCSVFileInfo = new FileInfo(otherCSVPath);
            if (!otherCSVFileInfo.Exists)
            {
                CrossRefDOIToGZFileCache.Build(crossRefDoiListFolderPath, dataFolderPath);
            }

        }



        public static async Task BuildSmallCache(string dataFolderPath, HashSet<string> doiSet, string mailAddress)
        {
            Console.WriteLine("Building CrossRefSmallCache [START]");

            



            var crossrefFolderInfo = CrossRefCacheBuilder.SearchCrossRefFolder(dataFolderPath + "/external");

            var otherCSVPath = dataFolderPath + "/auto_generated/cache/crossref_cache/doi_to_gzfile/others.csv";
            var otherCSVFileInfo = new FileInfo(otherCSVPath);
            if (!otherCSVFileInfo.Exists)
            {
                throw new Exception("others.csv not found");
            }

            // Build Found DOI Cache
            CrossRefFoundDOICache.Build(doiSet.ToList(), dataFolderPath, crossrefFolderInfo.FullName);


            var unknownDOISet = CSVFunctions.ReadCSVAsHashSet(dataFolderPath + "/auto_generated/cache/crossref_cache/unknown_doi.csv");
            await CrossRefExternalFoundDOICache.Build(dataFolderPath, doiSet, unknownDOISet, mailAddress);
            CSVFunctions.WriteCSV(dataFolderPath + "/auto_generated/cache/crossref_cache/unknown_doi.csv", unknownDOISet);






            Console.WriteLine("CrossRefSmallCache [END]");

        }

    }
}
