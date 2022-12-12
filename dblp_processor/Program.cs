using System;
using System.IO;
using System.Text;

using System.CommandLine;
using System.CommandLine.Invocation;


class Program
{
    static void Main(string[] args)
    {
        var xmlPathOption = new Option<string>(
            name: "--x",
            description: "XML Path"
            )
        { IsRequired = true };

        var urlPathOption = new Option<string>(
            name: "--u",
            description: "URL List Path.")
        { IsRequired = true };

        var outputPathOption = new Option<string>(
            name: "--o",
            description: "Output Path.")
        { IsRequired = true };

        var arXivPathOption = new Option<string>(
            name: "--i",
            description: "arXiv Path"
            )
        { IsRequired = true };


        var rootCommand = new RootCommand("Sample app for System.CommandLine");

        var dblpCommand = new Command("dblp", "Read and display the file.");
        var arXivCommand = new Command("arxiv", "Read and display the file.");

        dblpCommand.AddOption(xmlPathOption);
        dblpCommand.AddOption(urlPathOption);
        dblpCommand.AddOption(outputPathOption);

        arXivCommand.AddOption(arXivPathOption);
        arXivCommand.AddOption(outputPathOption);



        rootCommand.AddCommand(dblpCommand);
        rootCommand.AddCommand(arXivCommand);

        dblpCommand.SetHandler((_xmlPath, _urlPath, _outputPath) =>
            {
                DBLPProcessor.Processor.Process(_xmlPath, _urlPath, _outputPath);
                /*
                var folderPath = System.AppDomain.CurrentDomain.BaseDirectory;
                Console.WriteLine(folderPath);
                */
            },
            xmlPathOption, urlPathOption, outputPathOption);

        rootCommand.InvokeAsync(args);

        arXivCommand.SetHandler((_arxivPath, _outputPath) =>
            {
                ArxivProcessor.Processor.Process(_arxivPath, _outputPath);

            },
            arXivPathOption, outputPathOption);

        arXivCommand.InvokeAsync(args);


        /*
        return await rootCommand.InvokeAsync(args);
        */
    }

}