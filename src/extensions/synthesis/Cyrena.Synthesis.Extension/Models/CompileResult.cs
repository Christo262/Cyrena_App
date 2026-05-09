namespace Cyrena.Synthesis.Models
{
    internal class CompileResult
    {
        public bool Success { get; set; }
        public string Error { get; set; } = string.Empty;
        public byte[]? AssemblyBytes { get; set; }
    }
}
