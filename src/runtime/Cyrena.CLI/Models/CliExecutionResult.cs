namespace Cyrena.CLI.Models;

/// <summary>
/// Result of CLI command execution that indicates whether the main application should continue booting.
/// </summary>
public sealed class CliExecutionResult
{
    /// <summary>
    /// Indicates whether the main application should continue booting.
    /// </summary>
    public bool ShouldContinueBoot { get; init; }

    /// <summary>
    /// Optional exit code to return to the operating system.
    /// Only relevant when <see cref="ShouldContinueBoot"/> is false.
    /// </summary>
    public int? ExitCode { get; init; }

    /// <summary>
    /// Optional message to display before exiting or continuing.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Creates a result indicating the application should continue booting.
    /// </summary>
    public static CliExecutionResult Continue() => new() { ShouldContinueBoot = true };

    /// <summary>
    /// Creates a result indicating the application should stop booting with an optional exit code.
    /// </summary>
    /// <param name="exitCode">Optional exit code (defaults to 0).</param>
    /// <param name="message">Optional message to display.</param>
    public static CliExecutionResult Stop(int? exitCode = 0, string? message = null) => 
        new() { ShouldContinueBoot = false, ExitCode = exitCode, Message = message };

    /// <summary>
    /// Creates a result indicating a command was executed and the application should stop.
    /// </summary>
    /// <param name="message">Optional message to display.</param>
    public static CliExecutionResult CommandExecuted(string? message = null) => 
        new() { ShouldContinueBoot = false, ExitCode = 0, Message = message };

    /// <summary>
    /// Creates a result indicating no command was specified and the application should continue booting.
    /// </summary>
    public static CliExecutionResult NoCommand() => new() { ShouldContinueBoot = true };

    /// <summary>
    /// Creates a result indicating an unknown command was specified.
    /// </summary>
    /// <param name="commandName">The unknown command name.</param>
    public static CliExecutionResult UnknownCommand(string commandName) => 
        Stop(exitCode: 1, message: $"Unknown command: {commandName}");
}
