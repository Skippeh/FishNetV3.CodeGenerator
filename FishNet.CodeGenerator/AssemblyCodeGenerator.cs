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
        private static bool ResolverInitialized;
        
        public static ILPostProcessResult? ProcessFile(string assemblyPath, string outputPath, ICollection<string> assemblySearchPaths)
        {
            var compiledAssembly = ReadCompiledAssembly(assemblyPath);
            var processor = new FishNetILPP();
            processor.AssemblySearchPaths.AddRange(assemblySearchPaths.Select(path => PathRelativeToAssemblyPath(assemblyPath, path)));
            
            string[] extraSearchPaths = AppDomain.CurrentDomain.GetAssemblies()
                .Select(x => x.Location)
                .Select(Path.GetDirectoryName)
                .Distinct()
                .ToArray();

            processor.AssemblySearchPaths = processor.AssemblySearchPaths.Union(extraSearchPaths).ToList();

            string assemblyPathDir = Path.GetFullPath(Path.GetDirectoryName(assemblyPath)!);

            if (!processor.AssemblySearchPaths.Contains(assemblyPathDir))
                processor.AssemblySearchPaths.Add(assemblyPathDir);
            
            if (!ResolverInitialized)
            {
                ResolverInitialized = true;
                GeneratorAssemblyResolver.Initialize(processor.AssemblySearchPaths.ToList());
            }

            if (processor.WillProcess(compiledAssembly))
            {
                ILPostProcessResult? result = processor.Process(compiledAssembly);

                if (result == null)
                {
                    return null;
                }

                using var file = File.Create(outputPath);
                file.Write(result.InMemoryAssembly.PeData);

                if (result.InMemoryAssembly.PdbData is { Length: > 0 })
                {
                    using var pdbFile = File.Create(Path.ChangeExtension(outputPath, "pdb"));
                    pdbFile.Write(result.InMemoryAssembly.PdbData);
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
            Memory<byte> readBuffer = new Memory<byte>(new byte[fileStream.Length]);
            int numBytesRead = fileStream.Read(readBuffer.Span);
            byte[] fileBytes = readBuffer.Span.Slice(0, numBytesRead).ToArray();
            return fileBytes;
        }
    }
}