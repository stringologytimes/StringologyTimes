using System;
using System.IO;
using System.Text;
using System.Linq;
using CommandLine;
using System.Threading.Tasks;
using DataProcessor;
using System.Text.Json;
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


        [Option('o', "output", Required = true, HelpText = "Output Path")]
        public string OutputPath { get; set; } = "";
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
            var UrlPath = opts.DataFolderPath + "/auto_generated/url.csv";
            var TagPath = opts.DataFolderPath + "/auto_generated/tag.csv";
            var XmlPath = opts.DataFolderPath + "/external/dblp.xml";
            var JsonPath = opts.DataFolderPath + "/external/arxiv-metadata-oai-snapshot.json";
            var mailAddress = "takaaki.nishimoto@riken.jp";



            var doiToTagMapper = DoiToTagMapper.CreateDoiToTagMapper(UrlPath, TagPath);
            var primaryDOISet = new HashSet<string>(doiToTagMapper.Keys);

            await DOIElementPreprocessor.BuildCache(opts.DataFolderPath, mailAddress, primaryDOISet);


            var primaryDOIElementDict = DOIElementPreprocessor.BuildDOIElementDictionary(opts.DataFolderPath, primaryDOISet);

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
            await DOIElementPreprocessor.BuildCache(opts.DataFolderPath, mailAddress, secondaryDOISet);
            var secondaryDOIElementDict = DOIElementPreprocessor.BuildDOIElementDictionary(opts.DataFolderPath, secondaryDOISet);

            var resultFolderPath = opts.DataFolderPath + "/auto_generated/result";
            if (!Directory.Exists(resultFolderPath))
            {
                Directory.CreateDirectory(resultFolderPath);
            }

            DOIElement.Save(primaryDOIElementDict, resultFolderPath + "/primary_doi_elements.jsonl");
            DOIElement.Save(secondaryDOIElementDict, resultFolderPath + "/secondary_doi_elements.jsonl");

            var secondaryDOIList = secondaryDOISet.ToList();
            secondaryDOIList.Sort();
            var secondaryDOIListPath = resultFolderPath + "/secondary_doi.csv";
            using (var writer = new StreamWriter(secondaryDOIListPath, false, Encoding.UTF8))
            {
                secondaryDOIList.ForEach((v) =>
                {
                    writer.WriteLine(v);
                });
            }
            Console.WriteLine("Saved: " + secondaryDOIListPath);

            /*
            var primaryDOIElementPath = resultFolderPath + "/primary_doi_elements.jsonl";
            primaryDOIElements.Sort((a, b) => a.DOI.CompareTo(b.DOI));
            using (var writer = new StreamWriter(primaryDOIElementPath, false, Encoding.UTF8))
            {
                primaryDOIElements.ForEach((v) =>
                {
                    writer.WriteLine(v.to_JSON_Line());
                });
            }
            Console.WriteLine("Saved: " + primaryDOIElementPath);

            var secondaryDOIList = secondaryDOISet.ToList();
            secondaryDOIList.Sort();
            var secondaryDOIListPath = resultFolderPath + "/secondary_doi.csv";
            using (var writer = new StreamWriter(secondaryDOIListPath, false, Encoding.UTF8))
            {
                secondaryDOIList.ForEach((v) =>
                {
                    writer.WriteLine(v);
                });
            }
            Console.WriteLine("Saved: " + secondaryDOIListPath);
            */








            return 0;
        }

    }
}