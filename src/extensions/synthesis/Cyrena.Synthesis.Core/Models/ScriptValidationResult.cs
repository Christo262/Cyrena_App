using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Synthesis.Models
{
    /// <summary>
    /// Result of dynamic capability validation containing any detected violations.
    /// </summary>
    public class ScriptValidationResult
    {
        /// <summary>
        /// Whether the dynamic capability passed all validation checks.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// List of violations detected in the dynamic capability code.
        /// </summary>
        public IReadOnlyList<ScriptViolation> Violations { get; set; } = new List<ScriptViolation>();

        /// <summary>
        /// Human-readable summary of the validation result.
        /// </summary>
        public string Summary { get; set; } = string.Empty;
    }
}
