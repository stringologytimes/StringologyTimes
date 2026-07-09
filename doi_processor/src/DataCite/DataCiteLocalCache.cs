using System.Text;
using System.IO.Compression;
using System.Collections.ObjectModel;


namespace DataProcessor
{
    class DataCiteLocalCache
    {
        public static string GetCachePath(string dataFolderPath)
        {
            var f = new DirectoryInfo(dataFolderPath + "/auto_generated/cache/datacite_cache/small_cache");
            if (!f.Exists)
            {
                f.Create();
            }


            return dataFolderPath + "/auto_generated/cache/datacite_cache/small_cache/found_doi.jsonl";
        }
        public static Dictionary<string, string> Load(string dataFolderPath)
        {
            var dicPath = GetCachePath(dataFolderPath);
            var dic = JsonLib.LoadJSONLAsDictionary(dicPath, "id");
            return dic;
        }
        public static void UpdateDOICache(IDictionary<string, DOICacheInfo> doiCacheInfoDict, string dataFolderPath)
        {
            Dictionary<string, string> foundJSONLMap = Load(dataFolderPath);

            doiCacheInfoDict.Values.ToList().ForEach((v) =>
            {
                if (v.SourceCite == "DataCite")
                {
                    if (foundJSONLMap.ContainsKey(v.DOI))
                    {
                        v.SourceStatus = "LocalCache";
                        v.Date = DateTime.Now.ToString("yyyy-MM");
                    }

                }
            });
        }


        public static void Update(IDictionary<string, DOICacheInfo> doiCacheInfoDict, string dataFolderPath, string jsonlFolderPath)
        {

            var logFilePath = dataFolderPath + "/auto_generated/log/update_datacite_found_doi_cache.log";
            var logFile = new StreamWriter(logFilePath, true);
            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : Start");

            Console.WriteLine("Update Found DOI Cache(DataCite), DOI Count: " + doiCacheInfoDict.Count);
            var foundJSONLMapFilePath = GetCachePath(dataFolderPath);
            Dictionary<string, string> foundJSONLMap = DataCiteLocalCache.Load(dataFolderPath);


            Console.WriteLine("\t Found JSONL Map: " + foundJSONLMap.Count);

            var candidateDOISet = new HashSet<string>();
            var notDataCiteDOIPrefixSet = new HashSet<string>();
            var alreadyFoundDOISet = new HashSet<string>();


            Dictionary<string, HashSet<string>> doiPrefixToDoi = new Dictionary<string, HashSet<string>>();
            foreach (var doiCacheInfo in doiCacheInfoDict.Values)
            {
                var smallDOI = doiCacheInfo.DOI.ToLower();
                if (!foundJSONLMap.ContainsKey(smallDOI))
                {
                    if (doiCacheInfo.SourceCite == "DataCite")
                    {
                        if (doiCacheInfo.SourceStatus == "LocalCache" || doiCacheInfo.SourceStatus == "ExternalCache" || doiCacheInfo.SourceStatus == "NotFound")
                        {
                            alreadyFoundDOISet.Add(doiCacheInfo.DOI);
                        }
                        else
                        {
                            var doiPrefix = DOIFunctions.GetPrefix(smallDOI);
                            if (!doiPrefixToDoi.ContainsKey(doiPrefix))
                            {
                                doiPrefixToDoi[doiPrefix] = new HashSet<string>();
                            }
                            doiPrefixToDoi[doiPrefix].Add(smallDOI);
                            
                        }
                    }
                }
                else
                {
                    alreadyFoundDOISet.Add(smallDOI);
                }
            }


            alreadyFoundDOISet.ToList().ForEach((v) =>
            {
                logFile.WriteLine("Already found DOI: " + v);
            });
            notDataCiteDOIPrefixSet.ToList().ForEach((v) =>
            {
                logFile.WriteLine("Not DataCite DOI Prefix: " + v);
            });




            var doiPrefixMaxCount = doiPrefixToDoi.Count;
            var doiPrefixCounter = 0;
            var gzFilePathSet = new HashSet<string>();
            var othersHashSet = new HashSet<string>();

            foreach (var kvp in doiPrefixToDoi)
            {
                doiPrefixCounter++;

                var doiPrefix = kvp.Key;
                var filePath = $"{DataCiteDOIToGZFileCache.GetFolderPath(dataFolderPath)}/{doiPrefix}.tsv";
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Exists)
                {

                    //Console.WriteLine("\t Processing File: " + fileInfo.Name + "/" + doiPrefix);
                    var lines = File.ReadAllLines(filePath);

                    foreach (var line in lines)
                    {
                        var cols = line.Split("\t");
                        if (cols.Length == 3)
                        {
                            var lineDOI = cols[0];
                            var directoryName = cols[1];
                            var fileName = cols[2];
                            var gzFileName = directoryName + "/" + fileName;
                            if (kvp.Value.Contains(lineDOI))
                            {
                                gzFilePathSet.Add(jsonlFolderPath + "/dois/" + gzFileName);
                                candidateDOISet.Add(lineDOI);
                            }
                        }
                    }
                }
                else
                {
                    kvp.Value.ToList().ForEach((v) =>
                    {
                        othersHashSet.Add(v);

                    });
                }

            }
            Console.WriteLine();
            Console.WriteLine("\t Found JSONL Map: " + foundJSONLMap.Count);

            var othersFilePath = DataCiteDOIToGZFileCache.GetOthersFilePath(dataFolderPath);
            var othersFileInfo = new FileInfo(othersFilePath);
            if (othersFileInfo.Exists)
            {
                var lines = File.ReadAllLines(othersFilePath);
                foreach (var line in lines)
                {
                    var cols = line.Split("\t");
                    if (cols.Length == 3)
                    {
                        var lineDOI = cols[0];
                        var directoryName = cols[1];
                        var fileName = cols[2];
                        var gzFileName = directoryName + "/" + fileName;
                        if (othersHashSet.Contains(lineDOI))
                        {
                            gzFilePathSet.Add(jsonlFolderPath + "/dois/" + gzFileName);
                            candidateDOISet.Add(lineDOI);
                            othersHashSet.Remove(lineDOI);
                        }
                    }
                }
            }
            othersHashSet.ToList().ForEach((v) =>
            {
                logFile.WriteLine("Not found DOI (Type 1): " + v);
            });


            List<string> gzJSONLPaths = gzFilePathSet.ToList();
            var notFoundDOIs = new List<string>();
            CreateFoundJSONLFileSub(candidateDOISet.ToList(), gzJSONLPaths, foundJSONLMap, notFoundDOIs);
            notFoundDOIs.ForEach((v) =>
            {
                logFile.WriteLine("Not found DOI (Type 2): " + v);
            });

            using (var JSONLCacheWriter = new StreamWriter(GetCachePath(dataFolderPath), false, Encoding.UTF8))
            {
                foreach (var jsonl in foundJSONLMap.Values)
                {
                    JSONLCacheWriter.WriteLine(jsonl);
                }
            }

        }

        private static void CreateFoundJSONLFileSub(IReadOnlyList<string> dois, List<string> gzJSONLPaths, Dictionary<string, string> foundJSONLMap, List<string> notFoundDOIs)
        {
            HashSet<string> doiSet = new HashSet<string>(dois);
            var maxCount = gzJSONLPaths.Count;
            var counter = 0;
            gzJSONLPaths.ForEach((v) =>
            {
                counter++;
                var fileInfo = new FileInfo(v);
                Console.Write("\r\t\t Loading JSONL [" + counter + " / " + maxCount + "]");

                foreach (var line in JsonLib.ReadLinesFromGzip(v))
                {
                    var dict = JsonLib.CreateDictionaryFromJSONL(line);
                    var doi = dict["id"];
                    if (doiSet.Contains(doi))
                    {
                        foundJSONLMap[doi] = line;
                        doiSet.Remove(doi);
                    }
                }
            });

            notFoundDOIs.AddRange(doiSet.ToList());
            Console.WriteLine();
        }
    }
}