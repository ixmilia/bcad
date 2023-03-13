using System.CommandLine;
using System.CommandLine.Builder;
using System.IO;
using System.Linq;

namespace IxMilia.BCad.CommandLine
{
    public class CommandLineParser
    {
        public static CadArguments Parse(string[] args)
        {
            var cadArguments = args.Length > 0 && args[0] == "coreconsole"
                ? ParseFromConsoleArguments(args.Skip(1).ToArray())
                : ParseFromCadArguments(args);
            return cadArguments;
        }

        private static CadArguments ParseFromCadArguments(string[] args)
        {
            var rootCommand = new RootCommand();
            var drawingArgument = new Argument<FileInfo>(
                name: "drawing",
                getDefaultValue: () => null,
                description: "Drawing file to open.");
            var batchFileOption = new Option<FileInfo>(
                name: "/b",
                getDefaultValue: () => null,
                description: "File containing batch script commands");
            var errorLogOption = new Option<FileInfo>(
                name: "/e",
                getDefaultValue: () => null,
                description: "Location to write the error log");
            rootCommand.AddArgument(drawingArgument);
            rootCommand.AddOption(batchFileOption);
            rootCommand.AddOption(errorLogOption);
            var parser = new CommandLineBuilder(rootCommand).Build();
            var parseResult = parser.Parse(args);
            return new CadArguments(
                showUI: true,
                drawingFile: parseResult.GetValueForArgument(drawingArgument),
                batchFile: parseResult.GetValueForOption(batchFileOption),
                errorLog: parseResult.GetValueForOption(errorLogOption));
        }

        private static CadArguments ParseFromConsoleArguments(string[] args)
        {
            var rootCommand = new RootCommand();
            var drawingOption = new Option<FileInfo>(
                name: "/i",
                getDefaultValue: () => null,
                description: "Drawing file to open.");
            var batchFileOption = new Option<FileInfo>(
                name: "/s",
                getDefaultValue: () => null,
                description: "File containing batch script commands");
            var errorLogOption = new Option<FileInfo>(
                name: "/e",
                getDefaultValue: () => null,
                description: "Location to write the error log");
            rootCommand.AddOption(drawingOption);
            rootCommand.AddOption(batchFileOption);
            rootCommand.AddOption(errorLogOption);
            var parser = new CommandLineBuilder(rootCommand).Build();
            var parseResult = parser.Parse(args);
            return new CadArguments(
                showUI: false,
                drawingFile: parseResult.GetValueForOption(drawingOption),
                batchFile: parseResult.GetValueForOption(batchFileOption),
                errorLog: parseResult.GetValueForOption(errorLogOption));
        }
    }
}
