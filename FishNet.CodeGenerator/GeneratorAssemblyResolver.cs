using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FishNet.CodeGenerator;

internal static class GeneratorAssemblyResolver
{
    private static readonly Dictionary<string, Assembly> loadedAssemblies = new();
    
    internal static void Initialize(ICollection<string> searchPaths)
    {
        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            Console.WriteLine(
                $"Attempt to resolve assembly used by generator: {args.Name} (from {args.RequestingAssembly.Location})");

            // Name is a full assembly name such as "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
            // We only want the fist part before the comma.
            string fileName = $"{args.Name.Split(',')[0]}.dll";

            if (loadedAssemblies.TryGetValue(fileName, out var cachedAssembly))
                return cachedAssembly;

            foreach (string path in searchPaths)
            {
                string fullPath = System.IO.Path.Combine(path, fileName);

                if (System.IO.File.Exists(fullPath))
                {
                    var assembly = Assembly.LoadFile(fullPath);
                    loadedAssemblies.Add(fileName, assembly);
                    return assembly;
                }
            }

            return null;
        };
    }
}