namespace Cyrena.Synthesis.Contracts
{
    /// <summary>
    /// Structured logging API exposed to F# scripts.
    /// Scripts must use this API instead of Console.WriteLine or System.Diagnostics.
    /// </summary>
    public interface ICapabilityLogger
    {
        /// <summary>
        /// Logs a debug-level message. Use for detailed diagnostic information.
        /// </summary>
        void Debug(string message);

        /// <summary>
        /// Logs an informational message. Use for general script progress.
        /// </summary>
        void Info(string message);

        /// <summary>
        /// Logs a warning message. Use for recoverable issues or unexpected conditions.
        /// </summary>
        void Warn(string message);

        /// <summary>
        /// Logs an error message. Use for failures that affect script execution.
        /// </summary>
        void Error(string message);

        /// <summary>
        /// Logs an error message with exception details.
        /// </summary>
        void Error(string message, Exception exception);
    }
}
