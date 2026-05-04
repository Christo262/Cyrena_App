namespace MyConsoleApp.Contracts;

/// <summary>
/// Service that routes parsed CLI commands to their handlers.
/// </summary>
public interface ICliCommandService
{
    /// <summary>
    /// Executes the command described by the given arguments.
    /// </summary>
    /// <param name="args">The raw command-line arguments.</param>
    /// <returns>Exit code: 0 for success, non-zero for errors.</returns>
    int Execute(string[] args);

    /// <summary>
    /// Prints general help showing all available commands.
    /// </summary>
    void PrintHelp();
}
