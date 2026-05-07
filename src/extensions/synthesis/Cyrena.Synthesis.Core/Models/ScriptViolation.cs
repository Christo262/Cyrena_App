using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Synthesis.Models
{
    /// <summary>
    /// A single violation detected during dynamic capability validation.
    /// </summary>
    public class ScriptViolation
    {
        /// <summary>
        /// The type of violation (e.g., "RestrictedNamespace", "RestrictedType", "RestrictedDirective").
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// The specific pattern that was detected.
        /// </summary>
        public string Pattern { get; set; } = string.Empty;

        /// <summary>
        /// The line number where the violation was found (0-based).
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// The content of the line containing the violation.
        /// </summary>
        public string LineContent { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable explanation of why this is restricted.
        /// </summary>
        public string Reason { get; set; } = string.Empty;
    }
}
