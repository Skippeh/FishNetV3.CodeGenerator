using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FishNet.CodeGenerating.ILCore;
using Mono.Cecil;
using Unity.CompilationPipeline.Common.Diagnostics;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace FishNet.CodeGenerator
{
    public static class AssemblyCodeGenerator
    {
        public static ProcessResult ProcessFile(string assemblyPath, string outputPath)
        {
            var compiledAssembly = ReadCompiledAssembly(assemblyPath);
            var processor = new FishNetILPP();

            if (processor.WillProcess(compiledAssembly))
            {
                ILPostProcessResult? result = processor.Process(compiledAssembly);

                if (result == null)
                {
                    Console.Error.WriteLine("Failed to process (Process() returned null)");
                    return ProcessResult.UnknownError;
                }

                var builder = new StringBuilder();
                bool hasErrors = false;

                foreach (var diagnostic in result.Diagnostics)
                {
                    if (diagnostic.DiagnosticType == DiagnosticType.Error)
                    {
                        hasErrors = true;
                    }
                    
                    builder.AppendLine($"{diagnostic.File}:{diagnostic.Line}:{diagnostic.Column} {diagnostic.MessageData}");
                }

                if (builder.Length > 0)
                    Console.Error.WriteLine(builder.ToString());

                if (hasErrors)
                    return ProcessResult.HasDiagnosticErrors;

                using var file = File.Create(outputPath);
                file.Write(result.InMemoryAssembly.PeData);

                if (result.InMemoryAssembly.PdbData is { Length: > 0 })
                {
                    using var pdbFile = File.Create(Path.ChangeExtension(outputPath, "pdb"));
                    pdbFile.Write(result.InMemoryAssembly.PdbData);
                }
            }

            return ProcessResult.Ok;
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
                references.Add(reference.Name);
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