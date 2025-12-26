namespace DataProcessor
{
    class DataCiteJSONLLoader
    {

        public static DirectoryInfo SearchDataCiteFolder(string externalFolderPath)
        {
            DirectoryInfo di = new DirectoryInfo(externalFolderPath);
            if (di.Exists)
            {
                string DataCiteFilePath = "";

                foreach (var dir in Directory.GetDirectories(di.FullName)) // 直下のフォルダのみ
                {
                    var nameCheck = dir.IndexOf("DataCite") != -1;

                    if (nameCheck)
                    {
                        DataCiteFilePath = dir;
                    }
                    break;
                }
                if (DataCiteFilePath != "")
                {
                    return new DirectoryInfo(DataCiteFilePath);
                }
            }
            throw new Exception("DataCite folder not found");
        }
        public static string GetGZFileToDOIFolderPath(string dataFolderPath)
        {
            return dataFolderPath + "/auto_generated/datacite_cache/gzfile_to_doi";
        }
        public static string GetDOIToGZFileFolderPath(string dataFolderPath)
        {
            return dataFolderPath + "/auto_generated/datacite_cache/doi_to_gzfile";
        }
        public static HashSet<string> GetDOIPrefixSet(string dataFolderPath)
        {
            var path = GetDOIToGZFileFolderPath(dataFolderPath) + "/doi_prefix.csv";
            var fileInfo = new FileInfo(path);
            HashSet<string> doiPrefixSet = new HashSet<string>();
            if (fileInfo.Exists)
            {
                var lines = File.ReadAllLines(path);
                foreach (var line in lines)
                {
                    doiPrefixSet.Add(line);
                }
            }
            return doiPrefixSet;
        }

        public static Dictionary<string, string> Load(string foundJSONLMapFilePath)
        {
            Dictionary<string, string> foundJSONLMap = new Dictionary<string, string>();
            var foundJSONLMapFileInfo = new FileInfo(foundJSONLMapFilePath);
            if (foundJSONLMapFileInfo.Exists)
            {
                var jsonLString = File.ReadAllText(foundJSONLMapFilePath);
                var dicts = JsonLib.ProcessJSONL(jsonLString, true);
                Console.WriteLine("\t\t Loading Found JSONL Map: " + dicts.Count + " / " + jsonLString.Length);
                foreach (var dict in dicts)
                {
                    var doi = dict["id"];
                    foundJSONLMap[doi] = dict["input_line"];
                }
            }
            return foundJSONLMap;



        }

    }
}