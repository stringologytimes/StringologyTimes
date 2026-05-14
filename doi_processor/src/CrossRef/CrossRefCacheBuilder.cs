
namespace DataProcessor
{
    class CrossRefCacheBuilder
    {
        public static string GetGZFileToDoiFolderPath(string dataFolderPath)
        {
            return dataFolderPath + "/auto_generated/cache/crossref_cache/gzfile_to_doi";
        }
        public static string GetCrossRefCacheFolderPath(string dataFolderPath)
        {
            return dataFolderPath + "/auto_generated/cache/crossref_cache";
        }


        public static DirectoryInfo SearchCrossRefFolder(string externalFolderPath)
        {
            DirectoryInfo di = new DirectoryInfo(externalFolderPath);
            if (di.Exists)
            {
                string CrossRefFilePath = "";

                foreach (var dir in Directory.GetDirectories(di.FullName)) // 直下のフォルダのみ
                {
                    var containsGZ = false;
                    var nameCheck = dir.IndexOf("Crossref") != -1;


                    foreach (var file in Directory.EnumerateFiles(dir)) // 直下のファイルのみ
                    {
                        FileInfo fi = new FileInfo(file);

                        if (fi.Extension == ".gz")
                        {
                            containsGZ = true;
                        }
                    }
                    if (containsGZ && nameCheck)
                    {
                        CrossRefFilePath = dir;
                    }
                }
                if (CrossRefFilePath != "")
                {
                    return new DirectoryInfo(CrossRefFilePath);
                }
            }
            throw new Exception("CrossRef folder not found");
        }
        public static Dictionary<string, string> Load(string dicPath)
        {
            Dictionary<string, string> foundJSONLMap = new Dictionary<string, string>();
            var foundJSONLMapFileInfo = new FileInfo(dicPath);
            if (foundJSONLMapFileInfo.Exists)
            {
                var jsonLString = File.ReadAllText(dicPath);
                var dicts = JsonLib.ProcessJSONL(jsonLString, true);
                Console.WriteLine("\t\t Loading Found JSONL Map: " + dicts.Count + " / " + jsonLString.Length);
                foreach (var dict in dicts)
                {
                    var b = dict.ContainsKey("DOI");
                    if (!b)
                    {
                        dict.ToList().ForEach((v) => Console.WriteLine(v.Key + " : " + v.Value));
                        throw new Exception("DOI is not found");
                    }
                    var doi = dict["DOI"];
                    foundJSONLMap[doi] = dict["input_line"];
                }
            }
            return foundJSONLMap;
        }

    }
}
