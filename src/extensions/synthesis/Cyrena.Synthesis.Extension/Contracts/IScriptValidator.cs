using Cyrena.Synthesis.Models;

namespace Cyrena.Synthesis.Contracts
{
    /// <summary>
    /// Validates F# dynamic capability code for restricted patterns before compilation.
    /// This is a guardrail layer, NOT a security boundary.
    /// The true security boundary is the worker process isolation architecture.
    /// </summary>
    public interface IScriptValidator
    {
        /// <summary>
        /// Scans dynamic capability code for restricted patterns and returns validation results.
        /// </summary>
        Task<ScriptValidationResult> ValidateAsync(string code, CancellationToken cancellationToken = default);
    }
}
