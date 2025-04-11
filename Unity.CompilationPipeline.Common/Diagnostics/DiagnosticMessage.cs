namespace Unity.CompilationPipeline.Common.Diagnostics
{
    public class DiagnosticMessage
    {
        public string File { get; set; }
        public DiagnosticType DiagnosticType { get; set; }
        public string MessageData { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
    }
}