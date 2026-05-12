using System;
using System.IO;
using System.Text;
using System.Linq;
using CommandLine;
using System.Threading.Tasks;
using DataProcessor;
using System.Text.Json;
using System.IO.Compression;
namespace DataProcessor
{
    [Verb("dblp", HelpText = "Read and display the file.")]
    public class DBLPOptions
    {
        /*
        [Option('x', "xml", Required = true, HelpText = "DBLP XML Path")]
        public string XmlPath { get; set; } = "";
        [Option('j', "json", Required = false, HelpText = "Arxiv JSON Path")]
        public string JsonPath { get; set; } = "";

        [Option('c', "json-folder", Required = false, HelpText = "Arxiv JSON Folder Path")]
        public string JsonFolderPath { get; set; } = "";

        [Option('u', "url", Required = true, HelpText = "The Path to url.csv")]
        public string UrlPath { get; set; } = "";


        */

        [Option('d', "data", Required = true, HelpText = "The Path to data folder")]
        public string DataFolderPath { get; set; } = "";

        [Option('s', "skip_build", Required = true, HelpText = "The mode to run the program")]
        public bool SkipBuild { get; set; } = false;

        [Option('m', "mode", Required = true, HelpText = "XXX")]
        public string Mode { get; set; } = "";


        /*
        [Option('o', "output", Required = true, HelpText = "Output Path")]
        public string OutputPath { get; set; } = "";
        */
    }


    class Program
    {
        static async Task<int> Main(string[] args)
        {
            Console.WriteLine(String.Join(", ", args));

            return await Parser.Default.ParseArguments<DBLPOptions>(args)
                .MapResult(
                    (Func<DBLPOptions, Task<int>>)(opts => RunDBLP(opts)),
                    (Func<IEnumerable<Error>, Task<int>>)(errs => Task.FromResult(1))
                );
        }
        static string PrimaryDOIElementFileName = "primary_doi_elements.jsonl";
        static string SecondaryDOIElementFileName = "secondary_doi_elements.jsonl";
        static string SecondaryDOIListFileName = "secondary_doi.csv";

        static string ProcessedPrimaryDOIElementFileName = "processed_primary_doi_elements.jsonl";
        static string ProcessedSecondaryDOIElementFileName = "processed_secondary_doi_elements.jsonl";



        static async Task<int> BuildDOIElementDictionary(DBLPOptions opts)
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

        static async Task<int> RunDBLP(DBLPOptions opts)
        {
            var outputSystemMessageFunction = (string message) =>
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(message);
                Console.ResetColor();
            };

            if (opts.Mode == "dblp_proceedings_preprocessor")
            {
                outputSystemMessageFunction("Running dblp_processor");
                var dic = new Dictionary<string, List<string>>();
                var proceedingsSeriesDictionary = DataProcessor.DBLPProcessor.CollectProceedings(opts.DataFolderPath + "/external/dblp.xml",opts.DataFolderPath + "/raw/dblp/additional_booktitle.tsv");
                proceedingsSeriesDictionary.Save(opts.DataFolderPath + "/auto_generated/result/" + "dblp_proceedings.jsonl");

                return 0;
            }
            else if (opts.Mode == "dblp_proceedings_processor")
            {
                var proceedingsSeriesDictionary = DBLPProceedingsSeriesDictionary.Load(opts.DataFolderPath + "/auto_generated/result/" + "dblp_proceedings.jsonl");
                proceedingsSeriesDictionary.BuildDoiToBookTitleMapper();

                /*

                Console.WriteLine("Building prefix set");

                var bookTitle = proceedingsSeriesDictionary.DOIPrefixSearch("10.1007/978-3-030-59212-7_10");
                if (bookTitle != null)
                {
                Console.WriteLine("BookTitle: " + bookTitle);
                }
                else
                {
                    Console.WriteLine("BookTitle is null");
                }
                */
                /*

                var counter = 0;
                var counter2 = 0;

                proceedingsSeriesDictionary.Series.Values.ToList().ForEach((v) =>
                {
                    foreach (var element in v.Series)
                    {
                        var type = element.Value.GetTitleType();
                        if (type == 0)
                        {
                            Console.WriteLine(element.Value.Title);
                            Console.WriteLine(element.Value.BookTitle);
                            counter++;
                            if (counter > 100)
                            {
                                Console.WriteLine("counter: " + counter);
                                Console.WriteLine("counter2: " + counter2);
                                throw new Exception("Test");

                            }
                        }
                        else
                        {
                            counter2++;
                        }
                    }

                });



                var summaryList = new List<DBLPProceedingsSeriesSummary>();
                foreach (var element in proceedingsSeriesDictionary.Series)
                {
                    if (element.Value.CanSummarize())
                    {
                        {
                            var summary = DBLPProceedingsSeriesSummary.Build(element.Value);
                            summaryList.Add(summary);
                        }
                    }
                }
                DBLPProceedingsSeriesSummary.Save(summaryList, opts.DataFolderPath + "/auto_generated/result/" + "dblp_proceedings_summary.jsonl");
                    */



                return 0;

            }
            else
            {

                outputSystemMessageFunction("Running doi_processor");
                Console.WriteLine("SkipBuild: " + opts.SkipBuild);
                if (!opts.SkipBuild)
                {
                    await BuildDOIElementDictionary(opts);
                }
                else
                {
                    outputSystemMessageFunction("Skipping build of DOI element dictionary");
                }

                var primaryDOIElementDict = DOIElement.Load(opts.DataFolderPath + "/auto_generated/result/" + PrimaryDOIElementFileName, true);
                var secondaryDOIElementDict = DOIElement.Load(opts.DataFolderPath + "/auto_generated/result/" + SecondaryDOIElementFileName, true);

                outputSystemMessageFunction("Modifying container title using DBLP summary");
                ReplacementRules.ReplaceContainerTitleUsingDBLPSummary(opts.DataFolderPath + "/auto_generated/result/" + "dblp_proceedings.jsonl", primaryDOIElementDict, secondaryDOIElementDict);


                outputSystemMessageFunction("Normalizing container title");
                ContainerTitleNormalization.NormalizeDOIElementDictionary(primaryDOIElementDict);
                ContainerTitleNormalization.NormalizeDOIElementDictionary(secondaryDOIElementDict);


                outputSystemMessageFunction("Applying type replacement rules");
                ReplacementRules.ReplaceType(opts.DataFolderPath + "/raw/doi_processor/type_replacement_rules.tsv", primaryDOIElementDict, secondaryDOIElementDict);

                outputSystemMessageFunction("Applying container-title replacement rules");
                ReplacementRules.ReplaceContainerTitle(opts.DataFolderPath + "/raw/doi_processor/container_title_replacement_rules.tsv", primaryDOIElementDict, secondaryDOIElementDict);

                outputSystemMessageFunction("Modifying container title by DOI prefix");
                ReplacementRules.ReplaceContainerTitleByDOIPrefix(opts.DataFolderPath + "/raw/doi_processor/doi_prefix_key_container_title_value.tsv", primaryDOIElementDict, secondaryDOIElementDict);

                outputSystemMessageFunction("Modifying type by DOI prefix");
                ReplacementRules.ReplaceTypeByDOIPrefix(opts.DataFolderPath + "/raw/doi_processor/doi_prefix_key_type_value.tsv", primaryDOIElementDict, secondaryDOIElementDict);

                var resultFolderPath = opts.DataFolderPath + "/auto_generated/result";


                outputSystemMessageFunction("Saving primary DOI element dictionary to: " + resultFolderPath + "/" + ProcessedPrimaryDOIElementFileName);
                DOIElement.Save(primaryDOIElementDict, resultFolderPath + "/" + ProcessedPrimaryDOIElementFileName);

                outputSystemMessageFunction("Saving secondary DOI element dictionary to: " + resultFolderPath + "/" + ProcessedSecondaryDOIElementFileName);
                DOIElement.Save(secondaryDOIElementDict, resultFolderPath + "/" + ProcessedSecondaryDOIElementFileName);


                outputSystemMessageFunction("Building lightweight DOI element component");
                var lightweightDOIElementComponent = LightweightDOIElementComponent.Build(primaryDOIElementDict, secondaryDOIElementDict);

                outputSystemMessageFunction("Saving lightweight DOI element component to: " + resultFolderPath + "/" + "doi_info_parts");
                lightweightDOIElementComponent.OutputByGZip(resultFolderPath + "/" + "doi_info_parts");

                outputSystemMessageFunction("doi_processor is finished");


                return 0;


            }



        }
    }
}