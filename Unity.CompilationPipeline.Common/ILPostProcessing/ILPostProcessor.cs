namespace Unity.CompilationPipeline.Common.ILPostProcessing
{
    public abstract class ILPostProcessor
    {
        public abstract ILPostProcessor GetInstance();
        public abstract bool WillProcess(ICompiledAssembly compiledAssembly);
        public abstract ILPostProcessResult Process(ICompiledAssembly compiledAssembly);
    }
}