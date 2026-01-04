using System.Text;
using System.IO.Compression;
using System.Collections.ObjectModel;


namespace DataProcessor
{
    class CrossRefPreprocessor
    {

        public static async Task BuildCache(string dataFolderPath, HashSet<string> doiSet, string mailAddress)
        {


            var crossrefFolderInfo = CrossRefCacheBuilder.SearchCrossRefFolder(dataFolderPath + "/external");
            var crossRefDoiListFolderPath = dataFolderPath + "/auto_generated/cache/crossref_cache/gzfile_to_doi";
            //var crossRefResultFilePath = dataFolderPath + "/auto_generated/crossref_articles.jsonl";


            DataProcessor.CrossRefGZFileToDOICache.Build(crossrefFolderInfo.FullName, dataFolderPath);

            var otherCSVPath = dataFolderPath + "/auto_generated/cache/crossref_cache/doi_to_gzfile/others.csv";
            var otherCSVFileInfo = new FileInfo(otherCSVPath);
            if (!otherCSVFileInfo.Exists)
            {
                CrossRefDOIToGZFileCache.Build(crossRefDoiListFolderPath, dataFolderPath);
            }

            // Build Found DOI Cache
            CrossRefFoundDOICache.Build(doiSet.ToList(), dataFolderPath, crossrefFolderInfo.FullName);
            var unknownDOIList = await CrossRefExternalFoundDOICache.Build(dataFolderPath, doiSet, mailAddress);

            unknownDOIList.ForEach((v) =>
            {
                Console.WriteLine("Unknown DOI: " + v);
            });

        }

    }
}
