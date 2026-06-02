using System.Reflection;
using Cyrena.CLI.Contracts;
using Cyrena.CLI.Models;
using Cyrena.CLI.Services;

namespace Cyrena.CLI.Extensions;

/// <summary>
/// Extension methods for CLI service registration and execution.
/// </summary>
public static class CliServiceExtensions
{
    /// <summary>
    /// Creates and configures a new <see cref="CliService"/> instance.
    /// </summary>
    /// <param name="service">The service to configure.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The configured <see cref="CliService"/>.</returns>
    public static CliService Create(Action<CliService>? configure = null)
    {
        var service = new CliService();
        configure?.Invoke(service);
        return service;
    }

    /// <summary>
    /// Registers all CLI commands from the calling assembly.
    /// </summary>
    /// <param name="service">The CLI service.</param>
    /// <returns>The same service instance for chaining.</returns>
    public static CliService RegisterFromCurrentAssembly(this CliService service)
    {
        var assembly = Assembly.GetCallingAssembly();
        service.RegisterCommandsFromAssembly(assembly);
        return service;
    }

    /// <summary>
    /// Registers all CLI commands from the specified assembly.
    /// </summary>
    /// <param name="service">The CLI service.</param>
    /// <param name="assembly">The assembly to scan.</param>
    /// <returns>The same service instance for chaining.</returns>
    public static CliService RegisterFromAssembly(this CliService service, Assembly assembly)
    {
        service.RegisterCommandsFromAssembly(assembly);
        return service;
    }

    /// <summary>
    /// Registers all CLI commands from the specified type.
    /// </summary>
    /// <typeparam name="T">The type containing CLI commands.</typeparam>
    /// <param name="service">The CLI service.</param>
    /// <param name="instance">Optional instance for non-static command classes.</param>
    /// <returns>The same service instance for chaining.</returns>
    public static CliService RegisterFromType<T>(this CliService service, T? instance = null) where T : class
    {
        service.RegisterCommandsFromType(typeof(T), instance);
        return service;
    }

    /// <summary>
    /// Parses and executes CLI commands from command-line arguments.
    /// If no command is specified, returns a result indicating the application should continue booting.
    /// </summary>
    /// <param name="service">The CLI service.</param>
    /// <param name="args">The command-line arguments (e.g., args from Main).</param>
    /// <returns>Result indicating whether the main application should continue booting.</returns>
    public static CliExecutionResult ParseAndExecute(this CliService service, string[] args)
    {
        if (args.Length == 0)
        {
            // No command specified - allow boot to continue
            return CliExecutionResult.NoCommand();
        }

        var commandName = args[0];
        var commandArgs = args.Skip(1).ToArray();

        return service.Execute(commandName, commandArgs);
    }

    /// <summary>
    /// Parses and executes CLI commands, exiting the application if a command was executed.
    /// </summary>
    /// <param name="service">The CLI service.</param>
    /// <param name="args">The command-line arguments (e.g., args from Main).</param>
    /// <returns>True if the application should continue booting; otherwise, the application has exited.</returns>
    public static bool ParseAndExecuteWithExit(this CliService service, string[] args)
    {
        var result = service.ParseAndExecute(args);

        if (!result.ShouldContinueBoot)
        {
            if (!string.IsNullOrEmpty(result.Message))
            {
                Console.WriteLine(result.Message);
            }

            Environment.Exit(result.ExitCode ?? 0);
        }

        return true;
    }
}
