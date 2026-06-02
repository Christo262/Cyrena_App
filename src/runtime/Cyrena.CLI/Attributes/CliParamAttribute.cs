namespace Cyrena.CLI.Attributes;

/// <summary>
/// Marks a method parameter as a CLI parameter that can be bound from command-line arguments.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
public sealed class CliParamAttribute : Attribute
{
    /// <summary>
    /// The parameter name used in the CLI (e.g., "--name").
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Optional description of what the parameter does.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Indicates whether the parameter is required.
    /// </summary>
    public bool Required { get; set; } = false;

    /// <summary>
    /// Default value for the parameter if not provided.
    /// </summary>
    public object? DefaultValue { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CliParamAttribute"/> class.
    /// </summary>
    /// <param name="name">The parameter name used in the CLI.</param>
    public CliParamAttribute(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}
