using Cyrena.Synthesis.Models;

namespace Cyrena.Synthesis.Contracts
{
    /// <summary>
    /// Compiles and executes F# dynamic capabilities at runtime through the Cyrena controlled capability runtime.
    ///
    /// This is NOT a general unrestricted scripting engine. Dynamic capabilities must:
    /// - Operate through Cyrena APIs (ICyrenaScriptContext)
    /// - Be validated for restricted patterns before compilation
    /// - Have explicit permissions granted before execution
    /// - Use only approved assembly references
    ///
    /// Security model:
    /// - AssemblyLoadContext is NOT a security boundary
    /// - Planned: worker process isolation for true sandboxing
    ///
    /// Execution flow:
    /// 1. Cyrena builds a CapabilityRequest with typed arguments and permissions
    /// 2. Arguments are serialized into structured runtime data (ICyrenaArgs)
    /// 3. Runtime creates ICyrenaScriptContext with capability-gated APIs
    /// 4. Dynamic capability receives context object via entry point: let main (ctx: ICyrenaScriptContext) = ...
    /// 5. Dynamic capabilities access arguments through ctx.Args.GetString("name") etc.
    /// </summary>
    public interface IScriptEngine
    {
        /// <summary>
        /// Executes an F# dynamic capability with the provided structured request.
        /// The dynamic capability entry point must accept ICyrenaScriptContext.
        ///
        /// Before execution:
        /// 1. Dynamic capability is validated for restricted patterns
        /// 2. Permissions are checked against the request
        /// 3. A capability-gated context is constructed with typed arguments
        /// 4. Dynamic capability is compiled with restricted references
        /// 5. Dynamic capability executes with the Cyrena context
        ///
        /// Dynamic capabilities access arguments by name:
        ///   let filePath = ctx.Args.GetString("filePath")
        ///   let count = ctx.Args.GetInt32("count")
        ///   let enabled = ctx.Args.GetBoolean("enabled")
        ///
        /// Never use raw positional indexing like args.[0].
        /// </summary>
        /// <param name="script">The dynamic capability entity containing the code to execute.</param>
        /// <param name="request">The structured execution request with typed arguments and permissions.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task<CapabilityExecutionResult> ExecuteAsync(DynamicCapability script, CapabilityRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates that the provided F# code compiles without executing it.
        /// Also runs pattern validation for restricted APIs.
        /// </summary>
        Task<CapabilityExecutionResult> ValidateAsync(string code, CancellationToken cancellationToken = default);
    }
}
