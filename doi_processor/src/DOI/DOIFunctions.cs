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

        public static Dictionary<string, string> BuildMapperDOIToJSONL(string dicPath)
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
