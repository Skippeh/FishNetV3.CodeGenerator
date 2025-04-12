using System;
using CommandLine;

namespace FishNet.CodeGenerator.Cli;

public static class Program
{
    class LaunchArgs
    {
        [Option('i', "input", Required = true, HelpText = "Input assembly dll file")]
        public string InputFile { get; set; } = null!;

        [Option('o', "output", Required = false, HelpText = "Output dll file")]
        public string OutputFile { get; set; } = null!;
    }

    public static int Main(string[] args)
    {
        var parser = Parser.Default.ParseArguments<LaunchArgs>(args);
        return MainWithLaunchArgs(parser.Value);
    }

    private static int MainWithLaunchArgs(LaunchArgs args)
    {
        if (string.IsNullOrEmpty(args.OutputFile))
        {
            args.OutputFile = args.InputFile;
        }

        return (int)AssemblyCodeGenerator.ProcessFile(args.InputFile, args.OutputFile);
    }
}