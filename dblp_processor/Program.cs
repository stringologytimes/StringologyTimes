using System;
using System.IO;
using System.Text;
using CommandLine;

namespace DBLPProcessor
{
    [Verb("dblp", HelpText = "Read and display the file.")]
    public class DBLPOptions
    {
        [Option('x', "xml", Required = true, HelpText = "DBLP XML Path")]
        public string XmlPath { get; set; } = "";
        [Option('j', "json", Required = true, HelpText = "Arxiv JSON Path")]
        public string JsonPath { get; set; } = "";

        [Option('u', "url", Required = true, HelpText = "The Path to url.csv")]
        public string UrlPath { get; set; } = "";


        [Option('t', "tag", Required = true, HelpText = "The Path to tag.csv")]
        public string TagPath { get; set; } = "";


        [Option('o', "output", Required = true, HelpText = "Output Path")]
        public string OutputPath { get; set; } = "";
    }


    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine(String.Join(", ", args));

            return Parser.Default.ParseArguments<DBLPOptions>(args)
                .MapResult(
                    (DBLPOptions opts) => RunDBLP(opts),
                    errs => 1
                );
        }

        static int RunDBLP(DBLPOptions opts)
        {
            var doiToTagMapper = DoiToTagMapper.CreateDoiToTagMapper(opts.UrlPath, opts.TagPath);
            var dblpElements = DBLPProcessor.Processor.Process(opts.XmlPath, doiToTagMapper);
            var arxivArticles = ArxivProcessor.Processor.Process2(opts.JsonPath, doiToTagMapper);

            var mergedArticles = new List<DBLPElement>();
            dblpElements.Where((v) => v.BookTitleOrJournal != "CoRR").ToList().ForEach((v) => mergedArticles.Add(v));
            arxivArticles.ForEach((v) =>
            {
                var arxivDOI = v.getArxivDOI();
                var dblpElement = ArxivProcessor.ArxivArticle.toDBLPElement(v, doiToTagMapper[arxivDOI]);
                mergedArticles.Add(dblpElement);
            });

            var FoundDois = new HashSet<string>();
            foreach (var dblpElement in mergedArticles)
            {
                FoundDois.Add(dblpElement.DOI);
            }

            var notFoundDois = doiToTagMapper.Keys.ToList().Where((v) => !FoundDois.Contains(v)).ToList();
            foreach (var doi in notFoundDois)
            {
                Console.WriteLine("Not found: " + doi);
            }


            using var writer = new StreamWriter(opts.OutputPath, false, Encoding.UTF8);
            foreach (var dblpElement in mergedArticles)
            {
                var json = dblpElement.to_JSON_Line();
                writer.WriteLine(json);
            }
            Console.WriteLine("JSON lines saved to: " + opts.OutputPath);

            return 0;
        }

    }
}