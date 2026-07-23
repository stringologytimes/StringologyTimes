using System.Text.Json;
namespace DataProcessor
{
    class SemanticScholarLoader
    {

        public static Dictionary<string, string> Load(string foundJSONLMapFilePath)
        {
            CommonFunctions.OutputSystemMessageFunction("Loading from " + foundJSONLMapFilePath, ConsoleColor.Gray);
            Dictionary<string, string> foundJSONLMap = new Dictionary<string, string>();
            var foundJSONLMapFileInfo = new FileInfo(foundJSONLMapFilePath);
            if (foundJSONLMapFileInfo.Exists)
            {
                var jsonLString = File.ReadAllText(foundJSONLMapFilePath);
                jsonLString.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList().ForEach((v) =>
                {
                    var dict = JsonSerializer.Deserialize<SemanticScholarResult>(v);
                    if (dict != null)
                    {
                        foundJSONLMap[dict.DOI] = v;

                    }

                });
            }
            return foundJSONLMap;



        }

    }
}