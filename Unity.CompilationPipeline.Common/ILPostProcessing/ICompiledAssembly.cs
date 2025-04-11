namespace Unity.CompilationPipeline.Common.ILPostProcessing
{
    public interface ICompiledAssembly
    {
        InMemoryAssembly InMemoryAssembly { get; }
        string Name { get; }
        string[] References { get; }
        string[] Defines { get; }
    }
}