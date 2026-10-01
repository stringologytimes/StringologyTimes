namespace DataProcessor
{
    class DOIFunctions
    {
        public static string GetPrefix(string doi)
        {
            var parts = doi.Split("/");
            if (parts.Length >= 1)
            {
                return parts[0];
            }
            else
            {
                throw new Exception("Invalid DOI: " + doi);
            }

        }
        public static bool IsValidDOI(string doi)
        {
            bool b1 = doi.Contains(" ");
            bool b2 = doi.Contains("\t");
            bool b3 = doi.Contains("\n");
            bool b4 = doi.Contains("\r");
            return !b1 && !b2 && !b3 && !b4;
        }

        public static Dictionary<string, string> BuildMapperDOIToJSONL(string dicPath)
        {
            
            Dictionary<string, string> foundJSONLMap = new Dictionary<string, string>();
            var foundJSONLMapFileInfo = new FileInfo(dicPath);
            if (foundJSONLMapFileInfo.Exists)
            {
                var jsonLString = File.ReadAllText(dicPath);
                var dicts = JsonLib.ProcessJSONL(jsonLString, true);
                CommonFunctions.OutputSystemMessageFunction("Loading Map: " + dicPath, ConsoleColor.Gray);
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

        public static string CreateDummyDOI(string type, string title)
        {
            title = title.Replace(" ", "_");
            return "dummy/" + type + "/" + title.ToLower();
        }

    }
}
