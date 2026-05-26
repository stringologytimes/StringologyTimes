using System.Text;
using System.IO.Compression;
using System.Collections.ObjectModel;
using System.Collections.Generic;


namespace DataProcessor
{
    class CrossRefFoundDOICache
    {
        public static string GetCachePath(string dataFolderPath)
        {
            return dataFolderPath + "/auto_generated/cache/crossref_cache/small_cache/found_doi.jsonl";
        }
        public static Dictionary<string, string> Load(string dataFolderPath)
        {
            var dicPath = GetCachePath(dataFolderPath);
            var dic = DOIFunctions.BuildMapperDOIToJSONL(dicPath);
            return dic;
        }

        public static void Update(IReadOnlyList<string> dois, string dataFolderPath, string jsonlFolderPath)
        {
            var logFilePath = dataFolderPath + "/auto_generated/log/update_crossref_found_doi_cache.log";
            var logFile = new StreamWriter(logFilePath, true);
            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : Start");

            Console.WriteLine("Creating Found JSONL File(CrossRef): ");
            //var dicPath = GetCachePath(dataFolderPath);
            Dictionary<string, string> foundJSONLMap = Load(dataFolderPath);
            var crossRefDOIPrefixSet = CrossRefDOIToGZFileCache.GetDOIPrefixSet(dataFolderPath);
            Console.WriteLine("\t Found JSONL Map: " + foundJSONLMap.Count);

            var candidateDOISet = new HashSet<string>();
            var notCrossRefDOIPrefixSet = new HashSet<string>();
            var alreadyFoundDOISet = new HashSet<string>();


            //List<string> foundJSONLList = new List<string>();
            Dictionary<string, HashSet<string>> doiPrefixToDoi = new Dictionary<string, HashSet<string>>();
            foreach (var doi in dois)
            {
                var doiPrefix = DOIFunctions.GetPrefix(doi);
                if (!foundJSONLMap.ContainsKey(doi))
                {
                    if (crossRefDOIPrefixSet.Contains(doiPrefix))
                    {
                        if (!doiPrefixToDoi.ContainsKey(doiPrefix))
                        {
                            doiPrefixToDoi[doiPrefix] = new HashSet<string>();
                        }
                        doiPrefixToDoi[doiPrefix].Add(doi);
                    }
                    else
                    {
                        notCrossRefDOIPrefixSet.Add(doiPrefix);
                    }
                }
                else
                {
                    alreadyFoundDOISet.Add(doi);
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

            Console.WriteLine("\t DOI Count: " + " / " + dois.Count + " / " + foundJSONLMap.Count);

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
            CreateFoundJSONLFileSub(candidateDOISet.ToList(), gzJSONLPaths, foundJSONLMap, notFoundDOIs);

            notFoundDOIs.ForEach((v) =>
            {
                logFile.WriteLine("Not found DOI (Type 2): " + v);
            });


            var JSONLCacheWriter = new StreamWriter(GetCachePath(dataFolderPath), false, Encoding.UTF8);
            foreach (var jsonl in foundJSONLMap.Values)
            {
                JSONLCacheWriter.WriteLine(jsonl);
            }
            JSONLCacheWriter.Close();

            logFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : End");
            logFile.Close();
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

                if (counter % 50 == 0)
                {
                Console.Write("\r\t\t Loading JSONL [" + counter + " / " + maxCount + "]" + ", found articles: " + foundJSONLMap.Count);                    
                }


                foreach (var line in JsonLib.ReadLinesFromGzip(v))
                {
                    var dict = JsonLib.CreateDictionaryFromJSONL(line);
                    var doi = dict["DOI"];
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
