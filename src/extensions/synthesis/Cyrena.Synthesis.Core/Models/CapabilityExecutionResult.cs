using Cyrena.Models;
using System;
using System.Collections.Generic;

namespace Cyrena.Synthesis.Models
{
    /// <summary>
    /// Represents the result of executing an F# dynamic capability through the Cyrena controlled capability runtime.
    /// </summary>
    public class CapabilityExecutionResult : JsonStringObject
    {
        /// <summary>
        /// Whether the dynamic capability executed successfully.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The dynamic capability's output (stdout) combined with captured log output.
        /// </summary>
        public string Output { get; set; } = string.Empty;

        /// <summary>
        /// Any error output (stderr) or exception messages.
        /// </summary>
        public string Error { get; set; } = string.Empty;

        /// <summary>
        /// The return value of the dynamic capability, if any, serialized as a string.
        /// </summary>
        public string? ReturnValue { get; set; }

        public Dictionary<string, CapabilityResultValue> Results { get; set; } = new();

        /// <summary>
        /// How long the dynamic capability took to execute.
        /// </summary>
        public TimeSpan ExecutionTime { get; set; }

        /// <summary>
        /// When the execution started.
        /// </summary>
        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The ID of the dynamic capability that was executed.
        /// </summary>
        public string ScriptId { get; set; } = string.Empty;

        /// <summary>
        /// The structured arguments that were passed to the dynamic capability.
        /// Replaces the legacy string[] positional args.
        /// </summary>
        public Dictionary<string, string> Arguments { get; set; } = new();

        /// <summary>
        /// The permissions that were active during execution.
        /// </summary>
        public List<string> ActivePermissions { get; set; } = new();
    }
}
