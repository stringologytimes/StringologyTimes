namespace DataProcessor
{
    class CrossRefJSONLLoader
    {
        

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