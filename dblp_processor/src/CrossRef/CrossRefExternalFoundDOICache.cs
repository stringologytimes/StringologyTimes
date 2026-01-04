using System.Text;
using System.IO.Compression;
using System.Collections.ObjectModel;


namespace DataProcessor
{
    class CrossRefExternalFoundDOICache
    {
        public static string GetCachePath(string dataFolderPath)
        {
            return dataFolderPath + "/auto_generated/cache/crossref_cache/found_external_doi.jsonl";
        }
        public static Dictionary<string, string> Load(string dataFolderPath)
        {
            var crossRefExternalDicPath = GetCachePath(dataFolderPath);
            var crossRefExternalDic = DataProcessor.CrossRefCacheBuilder.Load(crossRefExternalDicPath);
            return crossRefExternalDic;
        }

        public static async Task<List<string>> Build(string dataFolderPath, HashSet<string> doiSet, string mailAddress)
        {
            var crossRefExternalDic = DataProcessor.CrossRefExternalFoundDOICache.Load(dataFolderPath);
            var crossRefDOIPrefixSet = DataProcessor.CrossRefDOIToGZFileCache.GetDOIPrefixSet(dataFolderPath);
            var foundDOICache = DataProcessor.CrossRefFoundDOICache.Load(dataFolderPath);

            var externalDOICandidateList = new List<string>();
            foreach (var doi in doiSet)
            {
                var doiPrefix = DOIFunctions.GetPrefix(doi);
                if (!foundDOICache.ContainsKey(doi) && crossRefDOIPrefixSet.Contains(doiPrefix) && !crossRefExternalDic.ContainsKey(doi))
                {
                    externalDOICandidateList.Add(doi);
                }
            }


            var map = await DataProcessor.CrossrefBulk.GetManyAsync(externalDOICandidateList, mailto: mailAddress);
            var unknownDOIList = new List<string>();

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
                        unknownDOIList.Add(doi);
                    }
                }
                else
                {
                    unknownDOIList.Add(doi);
                }
            }

            JsonLib.Save(crossRefExternalDic, GetCachePath(dataFolderPath));

            return unknownDOIList;

        }
    }
}