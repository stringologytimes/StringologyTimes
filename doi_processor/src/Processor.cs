using System.Xml;
using System.Xml.Linq;
using System.IO;
namespace DataProcessor
{
    class Processor
    {
        static string PrimaryDOIElementFileName = "primary_doi_elements.jsonl";
        static string SecondaryDOIElementFileName = "secondary_doi_elements.jsonl";
        static string SecondaryDOIListFileName = "secondary_doi.csv";

        static string ModifiedPrimaryDOIElementFileName = "modified_primary_doi_elements.jsonl";
        static string ModifiedSecondaryDOIElementFileName = "modified_secondary_doi_elements.jsonl";

        public static async Task<int> BuildDOIElementDictionary(DBLPOptions opts)
        {
            var outputSystemMessageFunction = (string message) =>
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(message);
                Console.ResetColor();
            };

            var UrlPath = opts.DataFolderPath + "/auto_generated/url.csv";
            var TagPath = opts.DataFolderPath + "/auto_generated/tag.csv";
            var mailAddress = "takaaki.nishimoto@riken.jp";

            outputSystemMessageFunction("Creating DOI to Tag Mapper");
            var doiToTagMapper = DoiToTagMapper.CreateDoiToTagMapper(UrlPath, TagPath);
            var primaryDOISet = new HashSet<string>(doiToTagMapper.Keys);

            outputSystemMessageFunction("Building cache for primary DOI elements");
            await DOIElementPreprocessor.BuildCache(opts.DataFolderPath, mailAddress, primaryDOISet);

            outputSystemMessageFunction("Building primary DOI element dictionary");
            var primaryDOIElementDict = DOIElementPreprocessor.BuildDOIElementDictionary(opts.DataFolderPath, primaryDOISet);

            outputSystemMessageFunction("Building secondary DOI set");
            var secondaryDOISet = new HashSet<string>();
            primaryDOIElementDict.Values.ToList().ForEach((v) =>
            {
                v.DOIReferences.ForEach((referenceDOI) =>
                {
                    if (!doiToTagMapper.ContainsKey(referenceDOI))
                    {
                        secondaryDOISet.Add(referenceDOI);
                    }
                });
            });
            outputSystemMessageFunction("Building cache for secondary DOI set");
            await DOIElementPreprocessor.BuildCache(opts.DataFolderPath, mailAddress, secondaryDOISet);

            outputSystemMessageFunction("Building secondary DOI element dictionary");
            var secondaryDOIElementDict = DOIElementPreprocessor.BuildDOIElementDictionary(opts.DataFolderPath, secondaryDOISet);

            var resultFolderPath = opts.DataFolderPath + "/auto_generated/result";
            if (!Directory.Exists(resultFolderPath))
            {
                Directory.CreateDirectory(resultFolderPath);
            }

            outputSystemMessageFunction("Saving primary DOI element dictionary to: " + resultFolderPath + "/" + PrimaryDOIElementFileName);
            DOIElement.Save(primaryDOIElementDict, resultFolderPath + "/" + PrimaryDOIElementFileName);

            outputSystemMessageFunction("Saving secondary DOI element dictionary to: " + resultFolderPath + "/" + SecondaryDOIElementFileName);
            DOIElement.Save(secondaryDOIElementDict, resultFolderPath + "/" + SecondaryDOIElementFileName);

            outputSystemMessageFunction("Saving secondary DOI list to: " + resultFolderPath + "/" + SecondaryDOIListFileName);
            var secondaryDOIList = secondaryDOISet.ToList();
            secondaryDOIList.Sort();
            CSVFunctions.WriteCSV(resultFolderPath + "/" + SecondaryDOIListFileName, secondaryDOIList);

            return 0;
        }
        public static void OutputSystemMessageFunction(string message)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static string GetFilePathInResultFolder(string dataFolderPath, string filename)
        {
            return dataFolderPath + "/auto_generated/result/" + filename;
        }
        public static int CreateLightweightDOIInfoFolder(DBLPOptions opts)
        {
            OutputSystemMessageFunction("Process: CreateLightweightDOIInfoFolder[Start]");

            var primaryDOIElementDict = DOIElement.Load(GetFilePathInResultFolder(opts.DataFolderPath, ModifiedPrimaryDOIElementFileName), true);
            var secondaryDOIElementDict = DOIElement.Load(GetFilePathInResultFolder(opts.DataFolderPath, ModifiedSecondaryDOIElementFileName), true);
            OutputSystemMessageFunction("Building lightweight DOI element component");
            var lightweightDOIElementComponent = LightweightDOIElementComponent.Build(primaryDOIElementDict, secondaryDOIElementDict);

            OutputSystemMessageFunction("Saving lightweight DOI element component to: " + GetFilePathInResultFolder(opts.DataFolderPath, "lightweight_doi_info"));
            lightweightDOIElementComponent.OutputByGZip(GetFilePathInResultFolder(opts.DataFolderPath, "lightweight_doi_info"));

            OutputSystemMessageFunction("Process: CreateLightweightDOIInfoFolder[End]");

            return 0;
        }

        public static int ModifyDOIElementDictionary(DBLPOptions opts)
        {

            OutputSystemMessageFunction("Running doi_processor");

            var primaryDOIElementDict = DOIElement.Load(GetFilePathInResultFolder(opts.DataFolderPath, PrimaryDOIElementFileName), true);
            var secondaryDOIElementDict = DOIElement.Load(GetFilePathInResultFolder(opts.DataFolderPath, SecondaryDOIElementFileName), true);

            OutputSystemMessageFunction("Modifying container title using DBLP summary");
            ReplacementRules.ReplaceContainerTitleUsingDBLPSummary(GetFilePathInResultFolder(opts.DataFolderPath, "dblp_proceedings.jsonl"), primaryDOIElementDict, secondaryDOIElementDict);


            OutputSystemMessageFunction("Normalizing container title");
            ContainerTitleNormalization.NormalizeDOIElementDictionary(primaryDOIElementDict);
            ContainerTitleNormalization.NormalizeDOIElementDictionary(secondaryDOIElementDict);


            OutputSystemMessageFunction("Applying type replacement rules");
            ReplacementRules.ReplaceType(opts.DataFolderPath + "/raw/doi_processor/type_replacement_rules.tsv", primaryDOIElementDict, secondaryDOIElementDict);

            OutputSystemMessageFunction("Applying container-title replacement rules");
            ReplacementRules.ReplaceContainerTitle(opts.DataFolderPath + "/raw/doi_processor/container_title_replacement_rules.tsv", primaryDOIElementDict, secondaryDOIElementDict);

            OutputSystemMessageFunction("Modifying container title by DOI prefix");
            ReplacementRules.ReplaceContainerTitleByDOIPrefix(opts.DataFolderPath + "/raw/doi_processor/doi_prefix_key_container_title_value.tsv", primaryDOIElementDict, secondaryDOIElementDict);

            OutputSystemMessageFunction("Modifying type by DOI prefix");
            ReplacementRules.ReplaceTypeByDOIPrefix(opts.DataFolderPath + "/raw/doi_processor/doi_prefix_key_type_value.tsv", primaryDOIElementDict, secondaryDOIElementDict);

            OutputSystemMessageFunction("Saving primary DOI element dictionary to: " + GetFilePathInResultFolder(opts.DataFolderPath, ModifiedPrimaryDOIElementFileName));
            DOIElement.Save(primaryDOIElementDict, GetFilePathInResultFolder(opts.DataFolderPath, ModifiedPrimaryDOIElementFileName));

            OutputSystemMessageFunction("Saving secondary DOI element dictionary to: " + GetFilePathInResultFolder(opts.DataFolderPath, ModifiedSecondaryDOIElementFileName));
            DOIElement.Save(secondaryDOIElementDict, GetFilePathInResultFolder(opts.DataFolderPath, ModifiedSecondaryDOIElementFileName));

            OutputSystemMessageFunction("doi_processor is finished");


            return 0;
        }

    }
}
