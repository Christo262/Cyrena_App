using System.Reflection;
using Cyrena.CLI.Attributes;

namespace Cyrena.CLI.Models;

/// <summary>
/// Describes a CLI parameter discovered from a method parameter with <see cref="CliParamAttribute"/>.
/// </summary>
public sealed class CliParameterDescriptor
{
    /// <summary>
    /// The parameter name used in the CLI (e.g., "name" for "--name").
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The parameter type.
    /// </summary>
    public Type ParameterType { get; init; } = typeof(void);

    /// <summary>
    /// The underlying parameter info from reflection.
    /// </summary>
    public ParameterInfo ParameterInfo { get; init; } = null!;

    /// <summary>
    /// Optional description of what the parameter does.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Indicates whether the parameter is required.
    /// </summary>
    public bool Required { get; init; }

    /// <summary>
    /// Default value for the parameter if not provided.
    /// </summary>
    public object? DefaultValue { get; init; }

    /// <summary>
    /// Indicates whether the parameter accepts multiple values (e.g., arrays or lists).
    /// </summary>
    public bool IsMultiValue { get; init; }

    internal static CliParameterDescriptor? Create(ParameterInfo parameter)
    {
        var attr = parameter.GetCustomAttribute<CliParamAttribute>();
        if (attr == null)
            return null;

        var paramType = parameter.ParameterType;
        var isMultiValue = paramType.IsArray || 
            (paramType.IsGenericType && paramType.GetGenericTypeDefinition() == typeof(List<>));

        return new CliParameterDescriptor
        {
            Name = attr.Name,
            ParameterType = paramType,
            ParameterInfo = parameter,
            Description = attr.Description,
            Required = attr.Required,
            DefaultValue = attr.DefaultValue,
            IsMultiValue = isMultiValue
        };
    }
}
