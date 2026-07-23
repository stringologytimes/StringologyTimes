using System.Text;
using System.IO.Compression;
using System.Collections.ObjectModel;
using System.Collections.Generic;


namespace DataProcessor
{
    class CrossRefLocalCache
    {
        public static string GetCachePath(string dataFolderPath)
        {
            var dirPath = dataFolderPath + "/auto_generated/cache/crossref_cache/small_cache";
            var dirPathFileInfo = new DirectoryInfo(dirPath);
            if (!dirPathFileInfo.Exists)
            {
                dirPathFileInfo.Create();
            }
            return dirPath + "/found_doi.jsonl";
        }
        public static Dictionary<string, string> Load(string dataFolderPath)
        {
            var dicPath = GetCachePath(dataFolderPath);
            var dic = DOIFunctions.BuildMapperDOIToJSONL(dicPath);
            return dic;
        }

        public static void UpdateDOICacheStatus(IDictionary<string, DOICacheInfo> doiCacheInfoDict, CrossRefSmallCache crossRefSmallCache)
        {

            doiCacheInfoDict.Values.ToList().ForEach((v) =>
            {
                if (crossRefSmallCache.localCacheDic.ContainsKey(v.DOI))
                {
                    v.SourceStatus = "LocalCache";
                    v.CacheCreatedDate = DateTime.Now.ToString("yyyy-MM");
                }
            });
        }


        public static void Update(IDictionary<string, DOICacheInfo> doiCacheInfoDict, CrossRefSmallCache crossRefSmallCache, string dataFolderPath, string jsonlFolderPath)
        {
            var logFilePath = dataFolderPath + "/auto_generated/log/update_crossref_found_doi_cache.log";
            var logFile = new StreamWriter(logFilePath, true);
            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : Start");

            CommonFunctions.OutputSystemMessageFunction("Update LocalCache(CrossRef) [START]");
            CommonFunctions.IncrementParagraphCounter();
            //var dicPath = GetCachePath(dataFolderPath);
            //Dictionary<string, string> foundJSONLMap = Load(dataFolderPath);
            //Console.WriteLine("\t Found JSONL Map: " + foundJSONLMap.Count);

            var candidateDOISet = new HashSet<string>();
            var notCrossRefDOIPrefixSet = new HashSet<string>();
            var alreadyFoundDOISet = new HashSet<string>();

            var count = 0;



            //List<string> foundJSONLList = new List<string>();
            Dictionary<string, HashSet<string>> doiPrefixToDoi = new Dictionary<string, HashSet<string>>();
            foreach (var doiCacheInfo in doiCacheInfoDict.Values)
            {
                if (doiCacheInfo.SourceCite == "CrossRef")
                {
                    if (doiCacheInfo.SourceStatus == "LocalCache" || doiCacheInfo.SourceStatus == "ExternalCache" || doiCacheInfo.SourceStatus == "NotFound")
                    {
                        alreadyFoundDOISet.Add(doiCacheInfo.DOI);
                    }
                    else
                    {
                        if (!crossRefSmallCache.localCacheDic.ContainsKey(doiCacheInfo.DOI))
                        {
                            var doiPrefix = DOIFunctions.GetPrefix(doiCacheInfo.DOI);
                            if (!doiPrefixToDoi.ContainsKey(doiPrefix))
                            {
                                doiPrefixToDoi[doiPrefix] = new HashSet<string>();
                            }
                            doiPrefixToDoi[doiPrefix].Add(doiCacheInfo.DOI);
                            count++;

                        }
                        else
                        {
                            alreadyFoundDOISet.Add(doiCacheInfo.DOI);
                        }

                    }
                }

            }


            alreadyFoundDOISet.ToList().ForEach((v) =>
            {
                logFile.WriteLine("Already found DOI: " + v);
            });
            notCrossRefDOIPrefixSet.ToList().ForEach((v) =>
            {
                logFile.WriteLine("Not CrossRef DOI Prefix: " + v);
            });


            var doiPrefixMacCount = doiPrefixToDoi.Count;
            var doiPrefixCounter = 0;
            var gzFilePathSet = new HashSet<string>();
            var othersHashSet = new HashSet<string>();

            //Console.WriteLine("\t DOI Count: " + " / " + dois.Count + " / " + foundJSONLMap.Count);

            foreach (var kvp in doiPrefixToDoi)
            {
                doiPrefixCounter++;

                var doiPrefix = kvp.Key;
                var doiSetForDoiPrefix = kvp.Value;

                var filePath = $"{CrossRefDOIToGZFileCache.GetDOIToGZFileFolderPath(dataFolderPath)}/{doiPrefix}.tsv";
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Exists)
                {
                    Console.Write("\r\t\t Processing DOI Prefix [" + doiPrefixCounter + " / " + doiPrefixMacCount + "]");

                    var lines = File.ReadAllLines(filePath);
                    foreach (var line in lines)
                    {
                        var cols = line.Split("\t");
                        if (cols.Length >= 2)
                        {
                            var lineDOI = cols[0];
                            var gzFileName = cols[1];
                            if (doiSetForDoiPrefix.Contains(lineDOI))
                            {
                                gzFilePathSet.Add(jsonlFolderPath + "/" + gzFileName);
                                logFile.WriteLine("Found DOI: " + lineDOI);
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
            var othersFilePath = CrossRefDOIToGZFileCache.GetOthersFilePath(dataFolderPath);
            var othersFileInfo = new FileInfo(othersFilePath);
            if (othersFileInfo.Exists)
            {
                var lines = File.ReadAllLines(othersFilePath);
                foreach (var line in lines)
                {
                    var cols = line.Split("\t");
                    if (cols.Length >= 2)
                    {
                        var lineDOI = cols[0];
                        var gzFileName = cols[1];
                        if (othersHashSet.Contains(lineDOI))
                        {
                            gzFilePathSet.Add(jsonlFolderPath + "/" + gzFileName);
                            logFile.WriteLine("Found DOI from others.tsv: " + lineDOI);
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



            var notFoundDOIs = new List<string>();


            List<string> gzJSONLPaths = gzFilePathSet.ToList();
            CreateFoundJSONLFileSub(candidateDOISet.ToList(), gzJSONLPaths, crossRefSmallCache.localCacheDic, notFoundDOIs);

            notFoundDOIs.ForEach((v) =>
            {
                logFile.WriteLine("Not found DOI (Type 2): " + v);
            });


            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : End");
            logFile.Close();

            CommonFunctions.DecrementParagraphCounter();
            CommonFunctions.OutputSystemMessageFunction("Update LocalCache(CrossRef) [END]");
        }
        private static void CreateFoundJSONLFileSub(IReadOnlyList<string> dois, List<string> gzJSONLPaths, Dictionary<string, string> foundJSONLMap, List<string> notFoundDOIs)
        {

            HashSet<string> doiSet = new HashSet<string>(dois);
            var doiSetCopy = new HashSet<string>(dois);


            var maxCount = gzJSONLPaths.Count;
            object lockObj = new object();

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 32 // 最大並列度を4に制限
            };
            System.Threading.Tasks.Parallel.For(0, gzJSONLPaths.Count, options, i =>
            {
                var gzFilePath = gzJSONLPaths[i];
                var fileInfo = new FileInfo(gzFilePath);




                foreach (var line in JsonLib.ReadLinesFromGzip(gzFilePath))
                {
                    var dict = JsonLib.CreateDictionaryFromJSONL(line);
                    var doi = dict["DOI"];
                    if (doiSet.Contains(doi))
                    {
                        lock (lockObj)
                        {
                            foundJSONLMap[doi] = line;
                            doiSetCopy.Remove(doi);

                            if (doiSetCopy.Count % 100 == 0)
                            {
                                Console.WriteLine("\t\t Loading JSONL [" + doiSetCopy.Count + " / " + doiSet.Count + "]");
                            }
                        }
                    }
                }
            });


            notFoundDOIs.AddRange(doiSetCopy.ToList());


            Console.WriteLine();

        }
    }
}
