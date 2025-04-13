using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.IO;
using System.Linq;

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

            if (externallyResolved)
                ;//continue;

            string? directory = Path.GetDirectoryName(fileName);

            if (directory == null)
                continue;

            if (!referenceFileDirectories.Contains(directory))
            {
                referenceFileDirectories.Add(directory);

                LogMessage(directory);
                foreach (string metadataName in refItem.MetadataNames)
                {
                    LogMessage($"- {metadataName}: {refItem.GetMetadata(metadataName)}");
                }
            }
        }

        if (AssemblySearchPaths != null)
        {
            referenceFileDirectories.AddRange(
                AssemblySearchPaths
                    .Select(x => x.GetMetadata("FullPath"))
                    .Where(x => x != null)
            );
        }

        var result = AssemblyCodeGenerator.ProcessFile(inputFile.FullName, inputFile.FullName, new ProcessOptions
        {
            AssemblySearchPaths = referenceFileDirectories
        });

        Log.LogError("Error to avoid having to edit TestPlugin before every test");
        return false;
    }

    private void LogMessage(string message)
    {
        Log.LogMessage(MessageImportance.High, message);
    }
}