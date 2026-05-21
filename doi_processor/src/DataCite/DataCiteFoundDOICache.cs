using System.Text;
using System.IO.Compression;
using System.Collections.ObjectModel;


namespace DataProcessor
{
    class DataCiteFoundDOICache
    {
        public static string GetCachePath(string dataFolderPath)
        {
            return dataFolderPath + "/auto_generated/cache/datacite_cache/found_doi.jsonl";
        }
        public static Dictionary<string, string> Load(string dataFolderPath)
        {
            var dicPath = GetCachePath(dataFolderPath);
            var dic = DataProcessor.DataCiteJSONLLoader.Load(dicPath);
            return dic;
        }
        public static void Update(List<string> dois, string dataFolderPath, string dataCiteFolderPath)
        {
            Console.WriteLine("Update Found DOI Cache(DataCite), DOI Count: " + dois.Count);
            var foundJSONLMapFilePath = GetCachePath(dataFolderPath);
            Dictionary<string, string> foundJSONLMap = DataCiteJSONLLoader.Load(foundJSONLMapFilePath);
            Console.WriteLine("\t Found JSONL Map: " + foundJSONLMap.Count);

            Dictionary<string, HashSet<string>> doiPrefixToDoi = new Dictionary<string, HashSet<string>>();
            foreach (var doi in dois)
            {
                var smallDOI = doi.ToLower();
                if (!foundJSONLMap.ContainsKey(smallDOI))
                {
                    var doiPrefix = DOIFunctions.GetPrefix(smallDOI);
                    if (!doiPrefixToDoi.ContainsKey(doiPrefix))
                    {
                        doiPrefixToDoi[doiPrefix] = new HashSet<string>();
                    }
                    doiPrefixToDoi[doiPrefix].Add(smallDOI.ToLower());
                }
            }
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
                            if (kvp.Value.Contains(lineDOI))                            {
                                gzFilePathSet.Add(dataCiteFolderPath + "/dois/" + gzFileName);
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
                            gzFilePathSet.Add(dataCiteFolderPath + "/dois/" + gzFileName);
                        }
                    }
                }
            }
            List<string> gzJSONLPaths = gzFilePathSet.ToList();
            CreateFoundJSONLFileSub(dois, gzJSONLPaths, foundJSONLMap);

            using (var JSONLCacheWriter = new StreamWriter(GetCachePath(dataFolderPath), false, Encoding.UTF8))
            {
                foreach (var jsonl in foundJSONLMap.Values)
                {
                    JSONLCacheWriter.WriteLine(jsonl);
                }
            }

        }

        private static void CreateFoundJSONLFileSub(List<string> dois, List<string> gzJSONLPaths, Dictionary<string, string> foundJSONLMap)
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
                    }
                }
            });
            Console.WriteLine();
        }
    }
}