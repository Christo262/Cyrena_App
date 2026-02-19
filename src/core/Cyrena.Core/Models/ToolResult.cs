using Newtonsoft.Json;

namespace Cyrena.Models
{
    public class ToolResult : IJsonSerializable
    {
        public ToolResult() { }
        public ToolResult(bool success, string? message)
        {
            Success = success;
            Message = message;
        }

        public bool Success { get; set; }
        public string? Message { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    public class ToolResult<T> : ToolResult
        where T : class
    {
        public ToolResult() { }
        public ToolResult(bool success, string? message) : base(success, message) { }
        public ToolResult(T result, bool success = true, string? message = null) : base(success, message)
        {
            Result = result;
        }
        public T? Result { get; set; }
    }
}
