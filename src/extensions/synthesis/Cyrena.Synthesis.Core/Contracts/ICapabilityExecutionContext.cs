namespace Cyrena.Synthesis.Contracts
{
    /// <summary>
    /// The primary runtime context exposed to F# dynamic capabilities.
    /// Dynamic capabilities receive this context as their entry point parameter and must use
    /// only the APIs exposed through this interface. Direct system access
    /// (System.IO, System.Net, System.Diagnostics, Reflection) is prohibited.
    ///
    /// Arguments are accessed by name through ctx.Args instead of raw positional indexing.
    /// </summary>
    public interface ICapabilityExecutionContext
    {
        /// <summary>
        /// Structured argument system. Dynamic capabilities access arguments by name
        /// with type-safe accessors (GetString, GetInt32, GetBoolean, GetJson).
        /// Never use raw positional indexing like args.[0].
        /// </summary>
        ICapabilityArgs Args { get; }
        /// <summary>
        /// Structured logging API for dynamic capability output and diagnostics.
        /// </summary>
        ICapabilityLogger Log { get; }

        ICapabilityResultWriter Result { get; }

        T? GetService<T>() where T : class;
        T GetRequiredService<T>() where T:class;
    }
}
