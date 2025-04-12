using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using Unity.CompilationPipeline.Common.Diagnostics;

namespace FishNet.CodeGenerator.Cli;

public static class Program
{
    class LaunchArgs
    {
        [Option('i', "input", Required = true, HelpText = "Input assembly dll file")]
        public string InputFile { get; set; } = null!;

        [Option('o', "output", Required = false, HelpText = "Output dll file")]
        public string? OutputFile { get; set; }

        [Option('a', "assemblySearchPaths", Required = false,
            HelpText = "Extra search paths for assemblies. Can be absolute or relative to input file")]
        public ICollection<string> AssemblySearchPaths { get; set; } = null!;
    }

    public static int Main(string[] args)
    {
        var parser = Parser.Default.ParseArguments<LaunchArgs>(args);

        if (parser.Errors.Any())
            return -1;

        return MainWithLaunchArgs(parser.Value);
    }

    private static int MainWithLaunchArgs(LaunchArgs args)
    {
        if (string.IsNullOrEmpty(args.OutputFile))
        {
            args.OutputFile = args.InputFile;
        }

        var result = AssemblyCodeGenerator.ProcessFile(args.InputFile, args.OutputFile, new ProcessOptions
        {
            AssemblySearchPaths = args.AssemblySearchPaths
        });

        if (result == null)
        {
            Console.Error.WriteLine("Failed to process (Process() returned null)");
            return (int)ProcessResult.UnknownError;
        }

        if (result.Diagnostics.All(d => d.DiagnosticType != DiagnosticType.Error))
            return (int)ProcessResult.Ok;

        foreach (var diagnostic in result.Diagnostics)
        {
            if (diagnostic.DiagnosticType == DiagnosticType.Error)
            {
                Console.Error.WriteLine(
                    $"[{diagnostic.DiagnosticType}] {diagnostic.File}:{diagnostic.Line}:{diagnostic.Column}: {diagnostic.MessageData}");
            }
        }

        return (int)ProcessResult.HasDiagnosticErrors;
    }
}