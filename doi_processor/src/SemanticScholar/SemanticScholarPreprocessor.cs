
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using System.Text.Json;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace DataProcessor
{
    class SemanticScholarPreprocessor
    {
        public static async Task PreprocessAll(Dictionary<string, DOIElement> doiElementDict, string dataFolderPath)
        {
            
            

            var semanticScholarCacheFolderPath = dataFolderPath + "/auto_generated/cache/semantic_scholar_cache";
            if (!Directory.Exists(semanticScholarCacheFolderPath))
            {
                Directory.CreateDirectory(semanticScholarCacheFolderPath);
            }
            var semanticScholarDicPath = dataFolderPath + "/auto_generated/cache/semantic_scholar_cache/doi_info.jsonl";
            var semanticScholarDic = SemanticScholarLoader.Load(semanticScholarDicPath);
            Console.WriteLine("Loaded: " + semanticScholarDic.Count + " / " + semanticScholarDicPath);

            var emptyReferenceList = new List<string>();
            foreach (var doiElement in doiElementDict.Values)
            {
                if (doiElement.DOIReferences.Count == 0 && !semanticScholarDic.ContainsKey(doiElement.DOI))
                {
                    emptyReferenceList.Add(doiElement.DOI);
                }                
            }


            /*
            emptyReferenceDoiDic.Keys.ToList().ForEach((v) =>
            {
                if (semanticScholarDic.ContainsKey(v))
                {
                    emptyReferenceDoiDic.Remove(v);
                }
            });
            */

            var emptyReferenceDoiDicList = emptyReferenceList.ToArray();
            var apiResults = await SemanticScholarClient.downloadReferrence(emptyReferenceDoiDicList);



            apiResults.ToList().ForEach((v) =>
            {
                var semanticScholarResult = SemanticScholarResult.ParseFromJSON(v);
                string json = JsonSerializer.Serialize(semanticScholarResult);
                semanticScholarDic[semanticScholarResult.DOI] = json;
            });
            JsonLib.Save(semanticScholarDic, semanticScholarDicPath);

            doiElementDict.Keys.ToList().ForEach((v) =>
            {
                var doiElement = doiElementDict[v];
                var doi = doiElement.DOI;
                if (doiElement.DOIReferences.Count == 0 && semanticScholarDic.ContainsKey(doi))
                {
                    var semanticScholarResult = JsonSerializer.Deserialize<SemanticScholarResult>(semanticScholarDic[doi]);
                    if (semanticScholarResult != null)
                    {
                        doiElement.DOIReferences.Clear();
                        doiElement.UnknownReferences.Clear();
                        semanticScholarResult.DOIReferences.ForEach((v) =>
                        {
                            doiElement.DOIReferences.Add(v);
                        });
                        semanticScholarResult.UnknownReferences.ForEach((v) =>
                        {
                            doiElement.UnknownReferences.Add(v);
                        });
                    }
                }
            });
        }
    }
}