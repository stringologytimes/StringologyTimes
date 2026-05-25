using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Collections.ObjectModel;
namespace DataProcessor
{
    class Processor
    {
        static string PRIMARY_DOI_ELEMENT_FILENAME = "primary_doi_elements.jsonl";
        static string SECONDARY_DOI_ELEMENT_FILENAME = "secondary_doi_elements.jsonl";
        static string SECONDARY_DOI_LIST_FILENAME = "secondary_doi.csv";

        static string MODIFIED_PRIMARY_DOI_ELEMENT_FILENAME = "modified_primary_doi_elements.jsonl";
        static string MODIFIED_SECONDARY_DOI_ELEMENT_FILENAME = "modified_secondary_doi_elements.jsonl";

        private static ReadOnlySet<string> BuildSecondaryDOISet(Dictionary<string, DOIElement> primaryDOIElementDict, string dataFolderPath)
        {


            var secondaryDOISet = new HashSet<string>();
            primaryDOIElementDict.Values.ToList().ForEach((v) =>
            {
                v.DOIReferences.ForEach((referenceDOI) =>
                {
                    if (!primaryDOIElementDict.ContainsKey(referenceDOI))
                    {
                        secondaryDOISet.Add(referenceDOI);
                    }
                    if (v.ContainerDOI.Length > 0 && !primaryDOIElementDict.ContainsKey(v.ContainerDOI))
                    {
                        secondaryDOISet.Add(v.ContainerDOI);
                    }

                });
            });

            var secondaryDOIList = secondaryDOISet.ToList();
            var specialContainerDOISet = DoiToTagMapper.CollectSpecialContainerDOI(dataFolderPath, secondaryDOIList, dataFolderPath + "/auto_generated/log/build_secondary_doi_set.log");

            specialContainerDOISet.ToList().ForEach((v) =>
            {
                if (!primaryDOIElementDict.ContainsKey(v))
                {
                    secondaryDOISet.Add(v);
                }
            });

            var readOnlySecondaryDOISet = new ReadOnlySet<string>(secondaryDOISet);
            return readOnlySecondaryDOISet;
        }



        public static void BuildTagCSVFromDataCite(DBLPOptions opts)
        {
            OutputSystemMessageFunction("Building DataCite subjects");
            var foundJSONLMapFilePath = DataCiteFoundDOICache.GetCachePath(opts.DataFolderPath);
            Dictionary<string, string> foundJSONLMap = DataCiteJSONLLoader.Load(foundJSONLMapFilePath);
            Console.WriteLine("Found JSONL Map: " + foundJSONLMap.Count);

            List<string> csvLines = new List<string>();

            var doiToTagMapper = DoiToTagMapper.CreateDoiToTagMapper(opts.DataFolderPath + "/raw");

            foundJSONLMap.Keys.ToList().ForEach((v) =>
            {
                if (doiToTagMapper.ContainsKey(v))
                {
                    var tags = DataCiteParser.GetTagsFromDataCiteJSONL(foundJSONLMap[v]);
                    if (tags.Count > 0)
                    {
                        List<string> line = new List<string>();
                        line.Add(v);
                        tags.ForEach((tag) => line.Add(tag));
                        csvLines.Add(string.Join('\t', line));
                    }

                }
            });


            var folderPath = opts.DataFolderPath + "/raw/user_files/DataCite";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            CSVFunctions.WriteCSV(folderPath + "/tag.tsv", csvLines);
            OutputSystemMessageFunction("Saved: " + folderPath + "/tag.tsv");

        }

        public static void BuildBigCache(DBLPOptions opts)
        {
            CrossRefCacheBuilder.BuildBigCache(opts.DataFolderPath);
            DataCitePreprocessor.BuildBigCache(opts.DataFolderPath);

        }


        public static async Task<int> BuildSmallCacheForPrimaryDOIElements(DBLPOptions opts)
        {
            OutputSystemMessageFunction("Creating DOI to Tag Mapper");
            var doiToTagMapper = DoiToTagMapper.CreateDoiToTagMapper(opts.DataFolderPath + "/raw");
            var primaryDOISet = new ReadOnlySet<string>(new HashSet<string>(doiToTagMapper.Keys));





            OutputSystemMessageFunction("Building cache for primary DOI elements");
            await DOIElementPreprocessor.BuildSmallCache(opts.DataFolderPath, opts.MailAddress, primaryDOISet, "primary_small_cache_hash.csv");

            return 0;
        }
        public static void BuildPrimaryDOIElementDictionary(DBLPOptions opts)
        {
            OutputSystemMessageFunction("Creating DOI to Tag Mapper");
            var doiToTagMapper = DoiToTagMapper.CreateDoiToTagMapper(opts.DataFolderPath + "/raw");
            var primaryDOISet = new ReadOnlySet<string>(new HashSet<string>(doiToTagMapper.Keys));

            Console.WriteLine("Primary DOI set: " + primaryDOISet.Count);

            OutputSystemMessageFunction("Building primary DOI element dictionary");
            var primaryDOIElementDict = DOIElementPreprocessor.BuildDOIElementDictionary(opts.DataFolderPath, primaryDOISet);

            OutputSystemMessageFunction("Saving primary DOI element dictionary to: " + GetFilePathInResultFolder(opts.DataFolderPath, PRIMARY_DOI_ELEMENT_FILENAME));
            DOIElement.Save(primaryDOIElementDict, GetFilePathInResultFolder(opts.DataFolderPath, PRIMARY_DOI_ELEMENT_FILENAME));

        }
        public static async Task<int> BuildSmallCacheForSecondaryDOIElements(DBLPOptions opts)
        {

            //OutputSystemMessageFunction("Creating DOI to Tag Mapper");
            //var doiToTagMapper = DoiToTagMapper.CreateDoiToTagMapper(opts.DataFolderPath + "/raw");
            //var primaryDOISet = new HashSet<string>(doiToTagMapper.Keys);


            OutputSystemMessageFunction("Loading primary DOI element dictionary");
            var primaryDOIElementDict = DOIElement.Load(GetFilePathInResultFolder(opts.DataFolderPath, PRIMARY_DOI_ELEMENT_FILENAME), true);


            OutputSystemMessageFunction("Building secondary DOI set");
            var secondaryDOISet = BuildSecondaryDOISet(primaryDOIElementDict, opts.DataFolderPath);

            OutputSystemMessageFunction("Building cache for secondary DOI set");
            await DOIElementPreprocessor.BuildSmallCache(opts.DataFolderPath, opts.MailAddress, secondaryDOISet, "secondary_small_cache_hash.csv");

            return 0;
        }

        public static void BuildSecondaryDOIElementDictionary(DBLPOptions opts)
        {

            OutputSystemMessageFunction("Creating DOI to Tag Mapper");
            var doiToTagMapper = DoiToTagMapper.CreateDoiToTagMapper(opts.DataFolderPath + "/raw");
            var primaryDOISet = new HashSet<string>(doiToTagMapper.Keys);

            OutputSystemMessageFunction("Loading primary DOI element dictionary");
            var primaryDOIElementDict = DOIElement.Load(GetFilePathInResultFolder(opts.DataFolderPath, PRIMARY_DOI_ELEMENT_FILENAME), true);

            OutputSystemMessageFunction("Building secondary DOI set");
            var secondaryDOISet = BuildSecondaryDOISet(primaryDOIElementDict, opts.DataFolderPath);

            OutputSystemMessageFunction("Building secondary DOI element dictionary");
            var secondaryDOIElementDict = DOIElementPreprocessor.BuildDOIElementDictionary(opts.DataFolderPath, secondaryDOISet);

            OutputSystemMessageFunction("Saving secondary DOI element dictionary to: " + GetFilePathInResultFolder(opts.DataFolderPath, SECONDARY_DOI_ELEMENT_FILENAME));
            DOIElement.Save(secondaryDOIElementDict, GetFilePathInResultFolder(opts.DataFolderPath, SECONDARY_DOI_ELEMENT_FILENAME));

            OutputSystemMessageFunction("Saving secondary DOI list to: " + GetFilePathInResultFolder(opts.DataFolderPath, SECONDARY_DOI_LIST_FILENAME));
            var secondaryDOIList = secondaryDOISet.ToList();
            secondaryDOIList.Sort();
            CSVFunctions.WriteCSV(GetFilePathInResultFolder(opts.DataFolderPath, SECONDARY_DOI_LIST_FILENAME), secondaryDOIList);

        }



        public static void OutputSystemMessageFunction(string message)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static string GetFilePathInResultFolder(string dataFolderPath, string filename)
        {
            var resultFolderPath = dataFolderPath + "/auto_generated/result";
            if (!Directory.Exists(resultFolderPath))
            {
                Directory.CreateDirectory(resultFolderPath);
            }


            return dataFolderPath + "/auto_generated/result/" + filename;
        }
        public static string GetFilePathInAutoGeneratedFolder(string dataFolderPath, string filename)
        {
            var autoGeneratedFolderPath = dataFolderPath + "/auto_generated";
            if (!Directory.Exists(autoGeneratedFolderPath))
            {
                Directory.CreateDirectory(autoGeneratedFolderPath);
            }

            return dataFolderPath + "/auto_generated/" + filename;
        }

        public static int CreateLightweightDOIInfoFolder(DBLPOptions opts)
        {
            OutputSystemMessageFunction("Process: CreateLightweightDOIInfoFolder[Start]");

            var primaryDOIElementDict = DOIElement.Load(GetFilePathInResultFolder(opts.DataFolderPath, MODIFIED_PRIMARY_DOI_ELEMENT_FILENAME), true);
            var secondaryDOIElementDict = DOIElement.Load(GetFilePathInResultFolder(opts.DataFolderPath, MODIFIED_SECONDARY_DOI_ELEMENT_FILENAME), true);
            OutputSystemMessageFunction("Building lightweight DOI element component");
            var lightweightDOIElementComponent = LightweightDOIElementComponent.Build(primaryDOIElementDict, secondaryDOIElementDict);

            OutputSystemMessageFunction("Saving lightweight DOI element component to: " + GetFilePathInResultFolder(opts.DataFolderPath, "lightweight_doi_info"));
            lightweightDOIElementComponent.OutputByGZip(GetFilePathInResultFolder(opts.DataFolderPath, "lightweight_doi_info"));

            OutputSystemMessageFunction("Process: CreateLightweightDOIInfoFolder[End]");

            return 0;
        }

        public static int ModifyDOIElementDictionary(DBLPOptions opts)
        {
            var logFolderPath = opts.DataFolderPath + "/auto_generated/log";

            OutputSystemMessageFunction("Running doi_processor");

            var primaryDOIElementDict = DOIElement.Load(GetFilePathInResultFolder(opts.DataFolderPath, PRIMARY_DOI_ELEMENT_FILENAME), true);
            var secondaryDOIElementDict = DOIElement.Load(GetFilePathInResultFolder(opts.DataFolderPath, SECONDARY_DOI_ELEMENT_FILENAME), true);

            OutputSystemMessageFunction("Modifying container title using DBLP summary");
            ReplacementRules.ReplaceContainerTitleUsingDBLPSummary(GetFilePathInResultFolder(opts.DataFolderPath, "dblp_proceedings.jsonl"), primaryDOIElementDict, secondaryDOIElementDict, logFolderPath);

            OutputSystemMessageFunction("Modifying series title using DBLP summary");
            var seriesTitleReplacementRulesPath = opts.DataFolderPath + "/raw/doi_processor/series_title_replacement_rules.tsv";
            ReplacementRules.ReplaceSeriesTitle(seriesTitleReplacementRulesPath, primaryDOIElementDict, secondaryDOIElementDict, logFolderPath);


            //OutputSystemMessageFunction("Normalizing container title");
            //ContainerTitleNormalization.NormalizeDOIElementDictionary(primaryDOIElementDict);
            //ContainerTitleNormalization.NormalizeDOIElementDictionary(secondaryDOIElementDict);


            OutputSystemMessageFunction("Applying type replacement rules");
            ReplacementRules.ReplaceType(opts.DataFolderPath + "/raw/doi_processor/type_replacement_rules.tsv", primaryDOIElementDict, secondaryDOIElementDict, logFolderPath);

            //OutputSystemMessageFunction("Applying container-title replacement rules");
            //ReplacementRules.ReplaceContainerTitle(opts.DataFolderPath + "/raw/doi_processor/container_title_replacement_rules.tsv", primaryDOIElementDict, secondaryDOIElementDict, logFolderPath);

            OutputSystemMessageFunction("Escaping container title");
            ReplacementRules.EscapeProcessing(primaryDOIElementDict, secondaryDOIElementDict, logFolderPath);

            OutputSystemMessageFunction("Modifying container DOI by DOI prefix");
            ReplacementRules.RpelaceContainerDOIByDOIPrefix(opts.DataFolderPath + "/raw/doi_processor/cotaniner_doi_replacement_rules.tsv", primaryDOIElementDict, secondaryDOIElementDict, logFolderPath);

            //OutputSystemMessageFunction("Modifying container title by DOI prefix");
            //ReplacementRules.ReplaceContainerTitleByDOIPrefix(opts.DataFolderPath + "/raw/doi_processor/doi_prefix_key_container_title_value.tsv", primaryDOIElementDict, secondaryDOIElementDict, logFolderPath);

            OutputSystemMessageFunction("Modifying type by DOI prefix");
            ReplacementRules.ReplaceTypeByDOIPrefix(opts.DataFolderPath + "/raw/doi_processor/doi_prefix_key_type_value.tsv", primaryDOIElementDict, secondaryDOIElementDict);

            OutputSystemMessageFunction("Appending tags to DOI element dictionary");
            ReplacementRules.AppendTags(opts.DataFolderPath, primaryDOIElementDict, secondaryDOIElementDict, logFolderPath);


            OutputSystemMessageFunction("Saving primary DOI element dictionary to: " + GetFilePathInResultFolder(opts.DataFolderPath, MODIFIED_PRIMARY_DOI_ELEMENT_FILENAME));
            DOIElement.Save(primaryDOIElementDict, GetFilePathInResultFolder(opts.DataFolderPath, MODIFIED_PRIMARY_DOI_ELEMENT_FILENAME));

            OutputSystemMessageFunction("Saving secondary DOI element dictionary to: " + GetFilePathInResultFolder(opts.DataFolderPath, MODIFIED_SECONDARY_DOI_ELEMENT_FILENAME));
            DOIElement.Save(secondaryDOIElementDict, GetFilePathInResultFolder(opts.DataFolderPath, MODIFIED_SECONDARY_DOI_ELEMENT_FILENAME));

            OutputSystemMessageFunction("doi_processor is finished");


            return 0;
        }

    }
}
