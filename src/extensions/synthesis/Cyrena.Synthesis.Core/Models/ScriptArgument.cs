using Cyrena.Models;

namespace Cyrena.Synthesis.Models
{
    /// <summary>
    /// Describes a single typed argument for a dynamic capability's entry point.
    /// Dynamic capabilities access arguments by name through ctx.Args instead of raw positional indexing.
    /// </summary>
    public class ScriptArgument : JsonStringObject
    {
        /// <summary>
        /// The name of the argument. Dynamic capabilities access this via ctx.Args.GetString("name").
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The data type of the argument: "string", "int", "bool", "double", "json".
        /// Used for type conversion and validation.
        /// </summary>
        public string Type { get; set; } = "string";

        /// <summary>
        /// The typed value of the argument. Stored as object for flexibility,
        /// serialized to string for the runtime context.
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// Description of what the argument represents and how it should be used.
        /// Used for execution previews and UI integration.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Whether this argument is required for execution.
        /// </summary>
        public bool IsRequired { get; set; } = true;

        /// <summary>
        /// An optional default value if the argument is not provided.
        /// </summary>
        public string? DefaultValue { get; set; }
    }
}
