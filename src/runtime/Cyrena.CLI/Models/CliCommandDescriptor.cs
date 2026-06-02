using System.Reflection;
using Cyrena.CLI.Attributes;

namespace Cyrena.CLI.Models;

/// <summary>
/// Describes a CLI command discovered from a method with <see cref="CliCommandAttribute"/>.
/// </summary>
public sealed class CliCommandDescriptor
{
    /// <summary>
    /// The command name used in the CLI (e.g., "some-command").
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Optional description of what the command does.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// The method info for the command handler.
    /// </summary>
    public MethodInfo Method { get; init; } = null!;

    /// <summary>
    /// The instance that contains the command method (for non-static methods).
    /// </summary>
    public object? Target { get; init; }

    /// <summary>
    /// The parameters accepted by this command.
    /// </summary>
    public IReadOnlyList<CliParameterDescriptor> Parameters { get; init; } = Array.Empty<CliParameterDescriptor>();

    /// <summary>
    /// Indicates whether the command method is static.
    /// </summary>
    public bool IsStatic => Method.IsStatic;

    /// <summary>
    /// Indicates whether the command returns a Task (async method).
    /// </summary>
    public bool IsAsync => Method.ReturnType == typeof(Task) || 
                           (Method.ReturnType.IsGenericType && Method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));

    /// <summary>
    /// Indicates whether the command returns CliExecutionResult (sync or async).
    /// </summary>
    public bool ReturnsResult => Method.ReturnType == typeof(CliExecutionResult) ||
                                 (IsAsync && Method.ReturnType.GetGenericArguments().FirstOrDefault() == typeof(CliExecutionResult));

    internal static CliCommandDescriptor? Create(MethodInfo method, object? target = null)
    {
        var attr = method.GetCustomAttribute<CliCommandAttribute>();
        if (attr == null)
            return null;

        // Validate method signature - only allow supported return types
        var returnType = method.ReturnType;
        
        // Valid return types: void, CliExecutionResult, Task, Task<CliExecutionResult>
        bool isValidReturnType = 
            returnType == typeof(void) ||
            returnType == typeof(CliExecutionResult) ||
            returnType == typeof(Task) ||
            (returnType.IsGenericType && 
             returnType.GetGenericTypeDefinition() == typeof(Task<>) && 
             returnType.GetGenericArguments()[0] == typeof(CliExecutionResult));

        if (!isValidReturnType)
        {
            throw new InvalidOperationException(
                $"CLI command '{attr.Name}' has invalid return type '{returnType.Name}'. " +
                $"Allowed return types are: void, CliExecutionResult, Task, or Task<CliExecutionResult>. " +
                $"Method: {method.DeclaringType?.Name}.{method.Name}");
        }

        var parameters = new List<CliParameterDescriptor>();
        foreach (var param in method.GetParameters())
        {
            var paramDesc = CliParameterDescriptor.Create(param);
            if (paramDesc != null)
                parameters.Add(paramDesc);
        }

        return new CliCommandDescriptor
        {
            Name = attr.Name,
            Description = attr.Description,
            Method = method,
            Target = target,
            Parameters = parameters.AsReadOnly()
        };
    }
}
