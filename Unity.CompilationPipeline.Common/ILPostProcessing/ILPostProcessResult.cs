using System.Collections.Generic;
using Unity.CompilationPipeline.Common.Diagnostics;

namespace Unity.CompilationPipeline.Common.ILPostProcessing
{
    public class ILPostProcessResult(
        InMemoryAssembly inMemoryAssembly,
        List<DiagnosticMessage> diagnostics)
    {
        public InMemoryAssembly InMemoryAssembly { get; set; } = inMemoryAssembly;
        public List<DiagnosticMessage> Diagnostics { get; set; } = diagnostics;

        public ILPostProcessResult(InMemoryAssembly inMemoryAssembly) : this(inMemoryAssembly, new List<DiagnosticMessage>())
        {
        }
    }
}