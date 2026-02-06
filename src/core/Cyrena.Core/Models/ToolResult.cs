using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Models
{
    public class ToolResult : JsonStringObject
    {
        public ToolResult() { }
        public ToolResult(bool success, string? message)
        {
            Success = success;
            Message = message;
        }

        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    public class ToolResult<T> : ToolResult
        where T : class
    {
        public ToolResult() { }
        public ToolResult(bool success, string? message):base(success, message) { }
        public ToolResult(T result, bool success = true, string? message = null):base(success, message)
        {
            Result = result;
        }
        public T? Result { get; set; }
    }
}
