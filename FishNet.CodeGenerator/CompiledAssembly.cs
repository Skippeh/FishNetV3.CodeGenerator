using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace FishNet.CodeGenerator;

public class CompiledAssembly : ICompiledAssembly
{
    public InMemoryAssembly? InMemoryAssembly { get; set; }
    public string? Name { get; set; }
    public string[]? References { get; set; }
    public string[]? Defines { get; set; }
}