using System.Text;
using System.IO.Compression;
using System.Collections.ObjectModel;


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
            Console.WriteLine("Creating Found JSONL File(CrossRef): ");
            //var dicPath = GetCachePath(dataFolderPath);
            Dictionary<string, string> foundJSONLMap = Load(dataFolderPath);
            var crsosRefDOIPrefixSet = CrossRefDOIToGZFileCache.GetDOIPrefixSet(dataFolderPath);
            Console.WriteLine("\t Found JSONL Map: " + foundJSONLMap.Count);


            //List<string> foundJSONLList = new List<string>();
            Dictionary<string, HashSet<string>> doiPrefixToDoi = new Dictionary<string, HashSet<string>>();
            foreach (var doi in dois)
            {
                var doiPrefix = DOIFunctions.GetPrefix(doi);
                if (!foundJSONLMap.ContainsKey(doi) && crsosRefDOIPrefixSet.Contains(doiPrefix))
                {
                    if (!doiPrefixToDoi.ContainsKey(doiPrefix))
                    {
                        doiPrefixToDoi[doiPrefix] = new HashSet<string>();
                    }
                    doiPrefixToDoi[doiPrefix].Add(doi);
                }
            }
            var doiPrefixMacCount = doiPrefixToDoi.Count;
            var doiPrefixCounter = 0;
            var gzFilePathSet = new HashSet<string>();
            var othersHashSet = new HashSet<string>();

            Console.WriteLine("\t DOI Count: "  + " / " + dois.Count + " / " + foundJSONLMap.Count);

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
                        }
                    }
                }
            }


            List<string> gzJSONLPaths = gzFilePathSet.ToList();
            CreateFoundJSONLFileSub(dois, gzJSONLPaths, foundJSONLMap);


            var JSONLCacheWriter = new StreamWriter(GetCachePath(dataFolderPath), false, Encoding.UTF8);
            foreach (var jsonl in foundJSONLMap.Values)
            {
                JSONLCacheWriter.WriteLine(jsonl);
            }
            JSONLCacheWriter.Close();
        }
        private static void CreateFoundJSONLFileSub(IReadOnlyList<string> dois, List<string> gzJSONLPaths, Dictionary<string, string> foundJSONLMap)
        {
            HashSet<string> doiSet = new HashSet<string>(dois);
            var maxCount = gzJSONLPaths.Count;
            var counter = 0;
            gzJSONLPaths.ForEach((v) =>
            {
                counter++;
                var fileInfo = new FileInfo(v);

                Console.Write("\r\t\t Loading JSONL [" + counter + " / " + maxCount + "]" + ", found articles: " + foundJSONLMap.Count);

                foreach (var line in JsonLib.ReadLinesFromGzip(v))
                {
                    var dict = JsonLib.CreateDictionaryFromJSONL(line);
                    var doi = dict["DOI"];
                    if (doiSet.Contains(doi))
                    {
                        foundJSONLMap[doi] = line;
                    }
                }

            });
            Console.WriteLine();

        }
    }
}
