using System.Text;
using System.IO.Compression;
using System.Collections.ObjectModel;
namespace DataProcessor
{
    class CrossRefExternalFoundDOICache
    {
        public static string GetCachePath(string dataFolderPath)
        {
            return dataFolderPath + "/auto_generated/cache/crossref_cache/small_cache/found_external_doi.jsonl";
        }
        public static Dictionary<string, string> Load(string dataFolderPath)
        {
            var crossRefExternalDicPath = GetCachePath(dataFolderPath);
            var crossRefExternalDic = DOIFunctions.BuildMapperDOIToJSONL(crossRefExternalDicPath);
            return crossRefExternalDic;
        }
        public static string GetUnknownDOIFilePath(string dataFolderPath)
        {
            var directoryInfo = new DirectoryInfo(dataFolderPath + "/auto_generated/cache/crossref_cache/small_cache");
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }
            return dataFolderPath + "/auto_generated/cache/crossref_cache/unknown_doi.tsv";
        }



        public static async Task Build(string dataFolderPath, ReadOnlySet<string> doiSet, HashSet<string> unknownDOISet, string mailAddress)
        {
            Console.WriteLine("Building CrossRefExternalFoundDOICache");
            Console.WriteLine("\t DOI Set: " + doiSet.Count);
            Console.WriteLine("\t Unknown DOI Set: " + unknownDOISet.Count);

            var crossRefExternalDic = DataProcessor.CrossRefExternalFoundDOICache.Load(dataFolderPath);
            var crossRefDOIPrefixSet = DataProcessor.CrossRefDOIToGZFileCache.GetDOIPrefixSet(dataFolderPath);
            var foundDOICache = DataProcessor.CrossRefFoundDOICache.Load(dataFolderPath);

            var externalDOICandidateList = new List<string>();
            foreach (var doi in doiSet)
            {
                var doiPrefix = DOIFunctions.GetPrefix(doi);
                if (!foundDOICache.ContainsKey(doi) && crossRefDOIPrefixSet.Contains(doiPrefix) && !crossRefExternalDic.ContainsKey(doi) && !unknownDOISet.Contains(doi))
                {
                    externalDOICandidateList.Add(doi);
                }
            }


            var map = await DataProcessor.CrossrefBulk.GetManyAsync(externalDOICandidateList, mailto: mailAddress);
            //var unknownDOIList = new List<string>();

            foreach (var (doi, json) in map)
            {
                if (json != null)
                {
                    var value = DataProcessor.JsonLib.GetValueFromJSONL(json, "message");
                    if (value != null)
                    {
                        crossRefExternalDic[doi] = value;
                    }
                    else
                    {
                        unknownDOISet.Add(doi);
                    }
                }
                else
                {
                    unknownDOISet.Add(doi);
                }
            }

            JsonLib.Save(crossRefExternalDic, GetCachePath(dataFolderPath));


        }
    }
}