
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
        public static async Task PreprocessAll(List<DOIElement> doiElements, string dataFolderPath)
        {
            var emptyReferenceDoiDic = new Dictionary<string, int>();
            for (int i = 0; i < doiElements.Count; i++)
            {
                if (doiElements[i].DOIReferences.Count == 0)
                {
                    emptyReferenceDoiDic[doiElements[i].DOI] = i;
                }
            }

            var semanticScholarCacheFolderPath = dataFolderPath + "/auto_generated/cache/semantic_scholar_cache";
            if (!Directory.Exists(semanticScholarCacheFolderPath))
            {
                Directory.CreateDirectory(semanticScholarCacheFolderPath);
            }
            var semanticScholarDicPath = dataFolderPath + "/auto_generated/cache/semantic_scholar_cache/doi_info.jsonl";
            var semanticScholarDic = SemanticScholarLoader.Load(semanticScholarDicPath);
            Console.WriteLine("Loaded: " + semanticScholarDic.Count + " / " + semanticScholarDicPath);

            emptyReferenceDoiDic.Keys.ToList().ForEach((v) =>
            {
                if (semanticScholarDic.ContainsKey(v))
                {
                    emptyReferenceDoiDic.Remove(v);
                }
            });

            var emptyReferenceDoiDicList = emptyReferenceDoiDic.Keys.ToArray();
            var apiResults = await SemanticScholarClient.downloadReferrence(emptyReferenceDoiDicList);



            apiResults.ToList().ForEach((v) =>
            {
                var semanticScholarResult = SemanticScholarResult.ParseFromJSON(v);
                string json = JsonSerializer.Serialize(semanticScholarResult);
                semanticScholarDic[semanticScholarResult.DOI] = json;
            });
            JsonLib.Save(semanticScholarDic, semanticScholarDicPath);

            for (int i = 0; i < doiElements.Count; i++)
            {
                var doi = doiElements[i].DOI;
                if (semanticScholarDic.ContainsKey(doi))
                {
                    var semanticScholarResult = JsonSerializer.Deserialize<SemanticScholarResult>(semanticScholarDic[doi]);
                    if (semanticScholarResult != null)
                    {
                        doiElements[i].DOIReferences.Clear();
                        doiElements[i].UnknownReferences.Clear();
                        semanticScholarResult.DOIReferences.ForEach((v) =>
                        {
                            doiElements[i].DOIReferences.Add(v);
                        });
                        semanticScholarResult.UnknownReferences.ForEach((v) =>
                        {
                            doiElements[i].UnknownReferences.Add(v);
                        });

                    }
                }
            }
        }
    }
}