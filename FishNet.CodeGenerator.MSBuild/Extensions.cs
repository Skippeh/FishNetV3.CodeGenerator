using System;
using Microsoft.Build.Framework;

namespace FishNet.CodeGenerator.MSBuild;

internal static class Extensions
{
    public static bool? GetMetadataBool(this ITaskItem taskItem, string key)
    {
        return taskItem.GetMetadata(key)?.Equals("true", StringComparison.InvariantCultureIgnoreCase);
    }
}