using Cyrena.Models;
using System;
using System.Collections.Generic;

namespace Cyrena.Synthesis.Models
{
    /// <summary>
    /// A structured request for dynamic capability execution.
    /// Contains the dynamic capability identifier, typed arguments, and required permissions.
    /// This replaces raw positional string[] args with a self-describing,
    /// permission-aware, validation-friendly execution model.
    /// </summary>
    public class CapabilityRequest : JsonStringObject
    {
        /// <summary>
        /// The unique identifier of the dynamic capability to execute.
        /// </summary>
        public string ScriptId { get; set; } = string.Empty;

        /// <summary>
        /// The typed arguments to pass to the dynamic capability.
        /// Dynamic capabilities access these via ctx.Args.GetString("name"), ctx.Args.GetInt32("name"), etc.
        /// </summary>
        public List<ScriptArgument> Arguments { get; set; } = new();

        /// <summary>
        /// The permissions required for this execution.
        /// The runtime will check these against granted permissions before execution.
        /// </summary>
        public List<CapabilityPermission> Permissions { get; set; } = new();

        /// <summary>
        /// Optional execution timeout override.
        /// If not set, uses the default from FXOptions.
        /// </summary>
        public TimeSpan? Timeout { get; set; }

        /// <summary>
        /// Whether to validate the dynamic capability before execution.
        /// Defaults to true.
        /// </summary>
        public bool ValidateBeforeExecution { get; set; } = true;

        /// <summary>
        /// Converts the typed arguments into a flat dictionary for the runtime context.
        /// </summary>
        public Dictionary<string, string> ToArgumentDictionary()
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var arg in Arguments)
            {
                if (arg.Value != null)
                {
                    result[arg.Name] = arg.Value.ToString() ?? string.Empty;
                }
            }
            return result;
        }
    }
}
