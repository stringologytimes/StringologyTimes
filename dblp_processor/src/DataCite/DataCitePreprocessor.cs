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








        public static async Task BuildCache(string dataFolderPath, HashSet<string> doiSet, string mailAddress)
        {
            var dataCiteDoiListFolderPath = dataFolderPath + "/auto_generated/cache/datacite_cache/gzfile_to_doi";

            var dataCiteFolderInfo = DataCiteJSONLLoader.SearchDataCiteFolder(dataFolderPath + "/external");

            DataProcessor.DataCiteGZFileToDOICache.Build(dataCiteFolderInfo.FullName, dataFolderPath);
            var dataCiteOtherCSVPath = dataFolderPath + "/auto_generated/cache/datacite_cache/doi_to_gzfile/others.csv";
            var dataCiteOtherCSVFileInfo = new FileInfo(dataCiteOtherCSVPath);
            if (!dataCiteOtherCSVFileInfo.Exists)
            {
                DataCiteDOIToGZFileCache.Build(dataCiteDoiListFolderPath, dataFolderPath);
            }

            DataCiteFoundDOICache.Build(doiSet.ToList(), dataFolderPath, dataCiteFolderInfo.FullName);

            var unknownDOIList = await DataCiteExternalFoundDOICache.Build(dataFolderPath, doiSet, mailAddress);

            unknownDOIList.ForEach((v) =>
            {
                Console.WriteLine("Unknown DOI: " + v);
            });



        }
    }
}
