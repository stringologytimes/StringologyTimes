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
            else if (opts.Mode == "build_doi_element_dictionary")
            {
                await Processor.BuildDOIElementDictionary(opts);
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