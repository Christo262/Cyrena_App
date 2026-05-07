using Cyrena.Models;
using System.ComponentModel.DataAnnotations;

namespace Cyrena.Synthesis.Models
{
    /// <summary>
    /// Represents a persisted F# dynamic capability with metadata for AI discoverability and execution.
    /// </summary>
    public class DynamicCapability : Entity
    {
        /// <summary>
        /// Human-readable title of the dynamic capability. Used for search and display.
        /// </summary>
        [Required]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Keywords for AI search and categorization.
        /// </summary>
        public List<string> Keywords { get; set; } = new();

        /// <summary>
        /// Detailed description of what the dynamic capability does.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The F# dynamic capability code. The entry point must accept ICyrenaScriptContext.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Metadata describing the expected arguments for the dynamic capability entry point.
        /// </summary>
        public List<ScriptArgument> Arguments { get; set; } = new();

        /// <summary>
        /// When the dynamic capability was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the dynamic capability was last modified.
        /// </summary>
        public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Whether the dynamic capability is enabled for execution.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// The version of the dynamic capability for tracking changes.
        /// </summary>
        public int Version { get; set; } = 1;
    }
}
