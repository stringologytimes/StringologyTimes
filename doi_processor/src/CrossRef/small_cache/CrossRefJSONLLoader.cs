namespace DataProcessor
{
    class CrossRefJSONLLoader
    {
        /*
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
        */
        

        public static Dictionary<string, string> LoadFoundDOI(string filepath)
        {
            return JsonLib.LoadJSONLAsDictionary(filepath, "DOI");
        }

        public static Dictionary<string, string> LoadFoundExternalDOI(string filepath)
        {
            return JsonLib.LoadJSONLAsDictionary(filepath, "DOI");
        }
        


    }
}