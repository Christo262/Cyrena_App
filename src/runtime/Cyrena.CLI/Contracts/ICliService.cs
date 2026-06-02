using System.Reflection;
using Cyrena.CLI.Models;

namespace Cyrena.CLI.Contracts;

/// <summary>
/// Service for discovering, registering, and executing CLI commands.
/// </summary>
public interface ICliService
{
    /// <summary>
    /// Discovers and registers all CLI commands from the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to scan for CLI commands.</param>
    void RegisterCommandsFromAssembly(Assembly assembly);

    /// <summary>
    /// Discovers and registers all CLI commands from the specified type.
    /// </summary>
    /// <param name="commandType">The type containing CLI command methods.</param>
    /// <param name="instance">Optional instance for non-static command classes.</param>
    void RegisterCommandsFromType(Type commandType, object? instance = null);

    /// <summary>
    /// Executes a CLI command with the provided arguments.
    /// </summary>
    /// <param name="commandName">The name of the command to execute.</param>
    /// <param name="args">The command-line arguments for the command.</param>
    /// <returns>Result indicating whether the main application should continue booting.</returns>
    CliExecutionResult Execute(string commandName, string[] args);

    /// <summary>
    /// Gets all registered commands.
    /// </summary>
    IReadOnlyList<CliCommandDescriptor> GetRegisteredCommands();

    /// <summary>
    /// Gets a specific command by name.
    /// </summary>
    /// <param name="commandName">The command name.</param>
    /// <returns>The command descriptor if found; otherwise, null.</returns>
    CliCommandDescriptor? GetCommand(string commandName);

    /// <summary>
    /// Generates help text for all registered commands or a specific command.
    /// </summary>
    /// <param name="commandName">Optional command name for detailed help. If null, shows all commands.</param>
    /// <returns>Formatted help text.</returns>
    string GetHelpText(string? commandName = null);
}
