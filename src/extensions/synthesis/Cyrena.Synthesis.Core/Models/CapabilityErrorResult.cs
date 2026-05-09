namespace Cyrena.Synthesis.Models
{
    public sealed class CapabilityErrorResult
    {
        public CapabilityErrorResult(string code, string message)
        {
            Code = code;
            Message = message;
        }

        public string Code { get; }
        public string Message { get; }
    }
}
