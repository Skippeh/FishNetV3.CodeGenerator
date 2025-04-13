using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace FishNet.CodeGenerating
{
    public class PostProcessorAssemblyResolver : BaseAssemblyResolver
    {
        public List<string> AssemblySearchPaths = [];

        private static readonly string[] IgnoreResolveAssemblies =
        [
        ];

        private readonly string[] m_AssemblyReferences;
        private readonly Dictionary<string, AssemblyDefinition> m_AssemblyCache = new Dictionary<string, AssemblyDefinition>();
        private readonly ICompiledAssembly m_CompiledAssembly;
        private AssemblyDefinition m_SelfAssembly;

        public PostProcessorAssemblyResolver(ICompiledAssembly compiledAssembly)
        {
            m_CompiledAssembly = compiledAssembly;
            m_AssemblyReferences = compiledAssembly.References;
        }

        public override AssemblyDefinition Resolve(AssemblyNameReference name) => Resolve(name, new ReaderParameters(ReadingMode.Deferred));

        public override AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            if (name.Name == "netstandard" || name.Name.StartsWith("System"))
                return Resolve(new AssemblyNameReference("mscorlib", new Version()));

            lock (m_AssemblyCache)
            {
                if (name.Name == m_CompiledAssembly.Name)
                {
                    return m_SelfAssembly;
                }

                var fileName = FindFile(name);
                bool pathResolved = false;

                if (fileName == null)
                {
                    fileName = ResolveAssemblyPath($"{name.Name}.dll");
                    pathResolved = true;
                }

                if (fileName == null)
                {
                    return base.Resolve(name, parameters);
                }

                // Try to resolve absolute file path to assembly
                if (!pathResolved)
                    fileName = ResolveAssemblyPath(fileName);

                var lastWriteTime = File.GetLastWriteTime(fileName);
                var cacheKey = $"{fileName}{lastWriteTime}";
                if (m_AssemblyCache.TryGetValue(cacheKey, out var result))
                {
                    return result;
                }

                parameters.AssemblyResolver = this;

                var ms = MemoryStreamFor(fileName);
                var pdb = $"{fileName}.pdb";
                if (File.Exists(pdb))
                {
                    parameters.SymbolStream = MemoryStreamFor(pdb);
                }

                var assemblyDefinition = AssemblyDefinition.ReadAssembly(ms, parameters);
                m_AssemblyCache.Add(cacheKey, assemblyDefinition);

                return assemblyDefinition;
            }
        }

        private string ResolveAssemblyPath(string assemblyPath)
        {
            if (Path.IsPathRooted(assemblyPath))
                return assemblyPath;

            if (IgnoreResolveAssemblies.Contains(assemblyPath))
            {
                var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return Path.Combine(currentDirectory!, assemblyPath);
            }

            foreach (var searchPath in AssemblySearchPaths)
            {
                var filePath = Path.Combine(searchPath, assemblyPath);

                if (File.Exists(filePath))
                {
                    Console.WriteLine($"Resolved assembly {assemblyPath}: {filePath}");
                    return filePath;
                }
            }

            Console.WriteLine($"Failed to resolve path to assembly: {assemblyPath}");
            return assemblyPath;
        }

        private string FindFile(AssemblyNameReference name)
        {
            var fileName = m_AssemblyReferences.FirstOrDefault(r => Path.GetFileName(r) == $"{name.Name}.dll");
            if (fileName != null)
            {
                return fileName;
            }

            // perhaps the type comes from an exe instead
            fileName = m_AssemblyReferences.FirstOrDefault(r => Path.GetFileName(r) == $"{name.Name}.exe");
            if (fileName != null)
            {
                return fileName;
            }

            //Unfortunately the current ICompiledAssembly API only provides direct references.
            //It is very much possible that a postprocessor ends up investigating a type in a directly
            //referenced assembly, that contains a field that is not in a directly referenced assembly.
            //if we don't do anything special for that situation, it will fail to resolve.  We should fix this
            //in the ILPostProcessing API. As a workaround, we rely on the fact here that the indirect references
            //are always located next to direct references, so we search in all directories of direct references we
            //got passed, and if we find the file in there, we resolve to it.
            return m_AssemblyReferences
                .Select(Path.GetDirectoryName)
                .Distinct()
                .Select(parentDir => Path.Combine(parentDir, $"{name.Name}.dll"))
                .FirstOrDefault(File.Exists);
        }

        private static MemoryStream MemoryStreamFor(string fileName)
        {
            return Retry(3, TimeSpan.FromSeconds(1), () =>
            {
                byte[] byteArray;
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    byteArray = new byte[fs.Length];
                    var readLength = fs.Read(byteArray, 0, (int)fs.Length);
                    if (readLength != fs.Length)
                    {
                        throw new InvalidOperationException("File read length is not full length of file.");
                    }
                }

                return new MemoryStream(byteArray);
            });
        }

        private static MemoryStream Retry(int retryCount, TimeSpan waitTime, Func<MemoryStream> func)
        {
            try
            {
                return func();
            }
            catch (IOException)
            {
                if (retryCount == 0)
                {
                    throw;
                }

                Console.WriteLine($"Caught IO Exception, trying {retryCount} more times");
                Thread.Sleep(waitTime);

                return Retry(retryCount - 1, waitTime, func);
            }
        }

        public void AddAssemblyDefinitionBeingOperatedOn(AssemblyDefinition assemblyDefinition)
        {
            m_SelfAssembly = assemblyDefinition;
        }
    }
}
