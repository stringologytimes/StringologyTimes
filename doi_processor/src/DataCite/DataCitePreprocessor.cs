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
using System.Collections.ObjectModel;
namespace DataProcessor
{
    class DataCitePreprocessor
    {
        public static string GetUnknownDOIFilePath(string dataFolderPath)
        {
            var f = new DirectoryInfo(dataFolderPath + "/auto_generated/cache/datacite_cache/small_cache");
            if (!f.Exists)
            {
                f.Create();
            }
            return dataFolderPath + "/auto_generated/cache/datacite_cache/small_cache/unknown_doi.tsv";
        }



        public static void BuildBigCache(string dataFolderPath)
        {
            var dataCiteDoiListFolderPath = DataCiteGZFileToDOICache.GetFolderPath(dataFolderPath);

            var dataCiteFolderInfo = DataCiteJSONLLoader.SearchDataCiteFolder(dataFolderPath + "/external");

            DataProcessor.DataCiteGZFileToDOICache.Build(dataCiteFolderInfo.FullName, dataFolderPath);
            var dataCiteOtherCSVPath = DataCiteDOIToGZFileCache.GetOthersFilePath(dataFolderPath);
            var dataCiteOtherCSVFileInfo = new FileInfo(dataCiteOtherCSVPath);
            if (!dataCiteOtherCSVFileInfo.Exists)
            {
                DataCiteDOIToGZFileCache.Build(dataCiteDoiListFolderPath, dataFolderPath);
            }

            DataCiteSubMapperBuilders.BuildISBNMapper(dataFolderPath);
            DataCiteSubMapperBuilders.BuildISSNMapper(dataFolderPath);
            DataCiteSubMapperBuilders.BuildTitleMapper(dataFolderPath);
            DataCiteSubMapperBuilders.BuildTypeListFile(dataFolderPath);

            //BuildBookCache(dataCiteDoiListFolderPath, dataFolderPath);
        }



        public static async Task UpdateSmallCache(string dataFolderPath, IDictionary<string, DOICacheInfo> doiCacheInfoDict, DataCiteSmallCache dataCiteSmallCache, string mailAddress)
        {
            CommonFunctions.OutputSystemMessageFunction("Updating SmallCache(DataCite) [START]");
            CommonFunctions.IncrementParagraphCounter();


            var dataCiteFolderInfo = DataCiteJSONLLoader.SearchDataCiteFolder(dataFolderPath + "/external");
            var dataCiteOtherCSVPath = DataCiteDOIToGZFileCache.GetOthersFilePath(dataFolderPath);

            var dataCiteOtherCSVFileInfo = new FileInfo(dataCiteOtherCSVPath);
            if (!dataCiteOtherCSVFileInfo.Exists)
            {
                throw new Exception("others.tsv not found");
            }

            // Build Found DOI Cache

            DataCiteLocalCache.Update(doiCacheInfoDict, dataCiteSmallCache, dataFolderPath, dataCiteFolderInfo.FullName);
            DataCiteLocalCache.UpdateDOICache(doiCacheInfoDict, dataCiteSmallCache);
            await DataCiteExternalFoundDOICache.Build(dataFolderPath, doiCacheInfoDict, dataCiteSmallCache, mailAddress);

            DataCiteExternalFoundDOICache.UpdateDOICache(doiCacheInfoDict, dataCiteSmallCache);


            CommonFunctions.DecrementParagraphCounter();
            CommonFunctions.OutputSystemMessageFunction("Updating SmallCache(DataCite) [END]");

        }





    }
}
