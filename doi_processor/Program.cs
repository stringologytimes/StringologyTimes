using System;
using System.IO;
using System.Text;
using System.Linq;
using CommandLine;
using System.Threading.Tasks;
using DataProcessor;
using System.Text.Json;
using System.IO.Compression;
namespace DBLPProcessor
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


        [Option('t', "tag", Required = true, HelpText = "The Path to tag.csv")]
        public string TagPath { get; set; } = "";
        */

        [Option('d', "data", Required = true, HelpText = "The Path to data folder")]
        public string DataFolderPath { get; set; } = "";


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
            var outputSystemMessageFunction = (string message) => {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(message);
                Console.ResetColor();
            };
            outputSystemMessageFunction("Running doi_processor");


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

            outputSystemMessageFunction("Applying type replacement rules");
            ReplacementRules.Apply1(opts.DataFolderPath + "/raw/doi_processor/type_replacement_rules.csv", primaryDOIElementDict, secondaryDOIElementDict);


            outputSystemMessageFunction("Saving primary DOI element dictionary to: " + resultFolderPath + "/primary_doi_elements.jsonl");
            DOIElement.Save(primaryDOIElementDict, resultFolderPath + "/primary_doi_elements.jsonl");

            outputSystemMessageFunction("Saving secondary DOI element dictionary to: " + resultFolderPath + "/secondary_doi_elements.jsonl");
            DOIElement.Save(secondaryDOIElementDict, resultFolderPath + "/secondary_doi_elements.jsonl");

            outputSystemMessageFunction("Saving secondary DOI list to: " + resultFolderPath + "/secondary_doi.csv");
            var secondaryDOIList = secondaryDOISet.ToList();
            secondaryDOIList.Sort();
            var secondaryDOIListPath = resultFolderPath + "/secondary_doi.csv";
            CSVFunctions.WriteCSV(secondaryDOIListPath, secondaryDOIList);


            outputSystemMessageFunction("Building lightweight DOI element component");
            var lightweightDOIElementComponent = LightweightDOIElementComponent.Build(primaryDOIElementDict, secondaryDOIElementDict);

            outputSystemMessageFunction("Saving lightweight DOI element component to: " + resultFolderPath + "/doi_info_parts");
            lightweightDOIElementComponent.OutputByGZip(resultFolderPath + "/doi_info_parts");

            outputSystemMessageFunction("doi_processor is finished");
       

            return 0;
        }

    }
}