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
    public ITaskItem? DontIncludeReferencePaths { get; set; }

    public override bool Execute()
    {
        FileInfo inputFile = new FileInfo(
            IntermediateOutputFilePath.GetMetadata("FullPath")
        );

        List<string> referenceFileDirectories = new(capacity: ReferencePath.Length);
        bool dontIncludeReferencePaths =
            DontIncludeReferencePaths?.GetMetadataBool("Identity") ?? false;

        if (!dontIncludeReferencePaths)
        {
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
        }

        if (AssemblySearchPaths != null)
        {
            foreach (var item in AssemblySearchPaths)
            {
                string? directory = item.GetMetadata("FullPath");

                if (directory == null)
                    continue;

                directory = GetFullPathOfDirectory(directory);

                if (!referenceFileDirectories.Contains(directory))
                    referenceFileDirectories.Insert(0, directory);
            }
        }
        else if (dontIncludeReferencePaths)
        {
            Log.LogError("Cannot generate code without reference paths. Either manually provide them or unset the FishNetCodeGenDontIncludeReferencePaths property.");
            return false;
        }

        var result = AssemblyCodeGenerator.ProcessFile(inputFile.FullName, inputFile.FullName, new ProcessOptions
        {
            AssemblySearchPaths = referenceFileDirectories
        });

        if (result == null)
        {
            Log.LogWarning("No FishNet related code found in assembly, skipping.");
            return true;
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

    private string GetFullPathOfDirectory(string directory)
    {
        if (File.Exists(directory))
            directory = Path.GetDirectoryName(directory)!;
            
        if (Path.IsPathRooted(directory))
            return directory;

        return Path.GetFullPath(directory);
    }

    private void LogMessage(string message)
    {
        Log.LogMessage(MessageImportance.High, message);
    }
}
