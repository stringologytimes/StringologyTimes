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

        [Option('d', "data", Required = true, HelpText = "The Path to data folder")]
        public string DataFolderPath { get; set; } = "";

        [Option('s', "skip_build", Required = true, HelpText = "The mode to run the program")]
        public bool SkipBuild { get; set; } = false;

        [Option('m', "mode", Required = true, HelpText = "XXX")]
        public string Mode { get; set; } = "";

        [Option('a', "mail_address", HelpText = "The mail address to use for the API")]
        public string MailAddress { get; set; } = "takaaki.nishimoto@riken.jp";

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
                var proceedingsSeriesDictionary = DataProcessor.DBLPProcessor.CollectProceedings(opts.DataFolderPath + "/external/dblp.xml", opts.DataFolderPath + "/raw/dblp/additional_booktitle.tsv");
                proceedingsSeriesDictionary.Save(opts.DataFolderPath + "/auto_generated/result/" + "dblp_proceedings.jsonl");

                return 0;
            }
            else if (opts.Mode == "dblp_proceedings_processor")
            {
                var proceedingsSeriesDictionary = DBLPProceedingsSeriesDictionary.Load(opts.DataFolderPath + "/auto_generated/result/" + "dblp_proceedings.jsonl");
                proceedingsSeriesDictionary.BuildDoiToBookTitleMapper();

                return 0;

            }
            else if (opts.Mode == "build_tag_csv_from_data_cite")
            {
                Processor.BuildTagCSVFromDataCite(opts);
                return 0;
            }
            else if (opts.Mode == "build_big_cache")
            {
                Processor.BuildBigCache(opts);
                return 0;
            }
            else if (opts.Mode == "build_small_cache_for_primary_doi_elements")
            {
                await Processor.BuildSmallCacheForPrimaryDOIElements(opts);
                return 0;
            }
            else if (opts.Mode == "build_primary_doi_element_dictionary")
            {
                Processor.BuildPrimaryDOIElementDictionary(opts);
                return 0;
            }
            else if (opts.Mode == "build_small_cache_for_secondary_doi_elements")
            {
                await Processor.BuildSmallCacheForSecondaryDOIElements(opts);
                return 0;
            }
            else if (opts.Mode == "build_secondary_doi_element_dictionary")
            {
                Processor.BuildSecondaryDOIElementDictionary(opts);
                return 0;
            }
            else if (opts.Mode == "create_lightweight_doi_info_folder")
            {
                Processor.CreateLightweightDOIInfoFolder(opts);
                return 0;
            }
            else
            {
                Processor.ModifyDOIElementDictionary(opts);
                return 0;

            }



        }
    }
}