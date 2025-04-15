using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.IO;
using System.Linq;
using Unity.CompilationPipeline.Common.Diagnostics;

namespace FishNet.CodeGenerator.MSBuild;

public class GenerateCodeTask : Task
{
    [Required] public ITaskItem IntermediateOutputFilePath { get; set; } = null!;
    public ITaskItem[]? AssemblySearchPaths { get; set; }
    [Required] public ITaskItem[] ReferencePath { get; set; } = null!;

    public override bool Execute()
    {
        FileInfo inputFile = new FileInfo(
            IntermediateOutputFilePath.GetMetadata("FullPath")
        );

        List<string> referenceFileDirectories = new(capacity: ReferencePath.Length);

        foreach (var refItem in ReferencePath)
        {
            string? fileName = refItem.GetMetadata("FullPath");
            bool externallyResolved = refItem.GetMetadataBool("ExternallyResolved") ?? false;

            string? directory = Path.GetDirectoryName(fileName);

            if (directory == null)
                continue;

            if (!referenceFileDirectories.Contains(directory))
            {
                if (externallyResolved)
                    referenceFileDirectories.Add(directory);
                else
                    referenceFileDirectories.Insert(0, directory);
            }
        }

        if (AssemblySearchPaths != null)
        {
            foreach (var item in AssemblySearchPaths)
            {
                string? directory = item.GetMetadata("FullPath");

                if (directory == null)
                    continue;

                directory = Path.GetFullPath(Path.GetDirectoryName(directory));

                if (!referenceFileDirectories.Contains(directory))
                    referenceFileDirectories.Insert(0, directory);
            }
        }

        var result = AssemblyCodeGenerator.ProcessFile(inputFile.FullName, inputFile.FullName, new ProcessOptions
        {
            AssemblySearchPaths = referenceFileDirectories
        });

        if (result == null)
        {
            Log.LogError("Unknown error occurred while processing assembly file");
            return false;
        }

        foreach (var diagnostic in result.Diagnostics)
        {
            if (diagnostic.DiagnosticType == DiagnosticType.Error)
                Log.LogError(null, null, null, diagnostic.File, diagnostic.Line, diagnostic.Column, diagnostic.Line, diagnostic.Column, diagnostic.MessageData);
            else
                Log.LogWarning(null, null, null, diagnostic.File, diagnostic.Line, diagnostic.Column, diagnostic.Line, diagnostic.Column, diagnostic.MessageData);
        }

        return result.Diagnostics.TrueForAll(d => d.DiagnosticType != DiagnosticType.Error);
    }

    private void LogMessage(string message)
    {
        Log.LogMessage(MessageImportance.High, message);
    }
}