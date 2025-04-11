namespace Unity.CompilationPipeline.Common.ILPostProcessing
{
    public class InMemoryAssembly(byte[] peData, byte[] pdbData)
    {
        public byte[] PeData { get; set; } = peData;
        public byte[] PdbData { get; set; } = pdbData;
    }
}