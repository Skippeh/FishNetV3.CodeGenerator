using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FishNet.CodeGenerating;
using FishNet.CodeGenerating.ILCore;
using Mono.Cecil;
using Unity.CompilationPipeline.Common.Diagnostics;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace FishNet.CodeGenerator
{
    public static class AssemblyCodeGenerator
    {
        private static bool AssemblyResolverInitialized;
        
        public static ILPostProcessResult? ProcessFile(string assemblyPath, string outputPath, ProcessOptions options)
        {
            var compiledAssembly = ReadCompiledAssembly(assemblyPath);
            var processor = new FishNetILPP();
            processor.AssemblySearchPaths.AddRange(options.AssemblySearchPaths.Select(path => PathRelativeToAssemblyPath(assemblyPath, path)));
            
            string[] extraSearchPaths = AppDomain.CurrentDomain.GetAssemblies()
                .Select(x => x.Location)
                .Select(Path.GetDirectoryName)
                .Distinct()
                .ToArray();

            processor.AssemblySearchPaths = processor.AssemblySearchPaths.Union(extraSearchPaths).ToList();

            string assemblyPathDir = Path.GetFullPath(Path.GetDirectoryName(assemblyPath)!);

            if (!processor.AssemblySearchPaths.Contains(assemblyPathDir))
                processor.AssemblySearchPaths.Add(assemblyPathDir);
            
            if (!AssemblyResolverInitialized)
            {
                AssemblyResolverInitialized = true;
                GeneratorAssemblyResolver.Initialize();
            }

            options = options with { AssemblySearchPaths = processor.AssemblySearchPaths };
            GeneratorAssemblyResolver.SetProcessOptions(options);
            
            Console.WriteLine($"Using {processor.AssemblySearchPaths.Count} assembly search paths:");
            foreach (string path in processor.AssemblySearchPaths)
            {
                System.Diagnostics.Debug.WriteLine($"- {path}");
            }

            if (processor.WillProcess(compiledAssembly))
            {
                Console.WriteLine($"Processing {assemblyPath}");
                
                ILPostProcessResult? result = processor.Process(compiledAssembly);

                if (result == null)
                {
                    return null;
                }

                using var file = File.Create(outputPath);
                file.Write(result.InMemoryAssembly.PeData, 0, result.InMemoryAssembly.PeData.Length);

                if (result.InMemoryAssembly.PdbData is { Length: > 0 })
                {
                    using var pdbFile = File.Create(Path.ChangeExtension(outputPath, "pdb"));
                    pdbFile.Write(result.InMemoryAssembly.PdbData, 0, result.InMemoryAssembly.PdbData.Length);
                }
                
                return result;
            }

            return null;
        }

        private static string PathRelativeToAssemblyPath(string assemblyPath, string searchDir)
        {
            if (File.Exists(searchDir))
                searchDir = Path.GetDirectoryName(searchDir)!;
            
            if (Path.IsPathRooted(searchDir))
                return searchDir;

            return Path.Combine(Path.GetFullPath(Path.GetDirectoryName(assemblyPath)!), searchDir);
        }

        private static CompiledAssembly ReadCompiledAssembly(string filePath)
        {
            var fileBytes = ReadFileBytes(filePath);
            
            string pdbPath = Path.ChangeExtension(filePath, "pdb");
            byte[]? pdbFileBytes = File.Exists(pdbPath) ? ReadFileBytes(pdbPath) : null;

            if (pdbFileBytes == null)
            {
                throw new FileNotFoundException("PDB file not found", pdbPath);
            }

            AssemblyDefinition assemblyDef = AssemblyDefinition.ReadAssembly(new MemoryStream(fileBytes));
            string assemblyName = assemblyDef.Name.Name;
            List<string> references = [];

            foreach (AssemblyNameReference reference in assemblyDef.MainModule.AssemblyReferences)
            {
                references.Add($"{reference.Name}.dll");
            }

            var compiledAssembly = new CompiledAssembly
            {
                InMemoryAssembly = new(peData: fileBytes, pdbData: pdbFileBytes),
                Name = assemblyName,
                References = references.ToArray()
            };

            return compiledAssembly;
        }

        private static byte[] ReadFileBytes(string filePath)
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var readBuffer = new byte[fileStream.Length];
            int numBytesRead = fileStream.Read(readBuffer, 0, readBuffer.Length);
            byte[] fileBytes = readBuffer.Take(numBytesRead).ToArray();
            return fileBytes;
        }
    }
}