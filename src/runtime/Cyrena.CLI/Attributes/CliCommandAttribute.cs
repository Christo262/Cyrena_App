namespace Cyrena.CLI.Attributes;

/// <summary>
/// Marks a method as a CLI command that can be auto-discovered and executed.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class CliCommandAttribute : Attribute
{
    /// <summary>
    /// The command name used in the CLI (e.g., "some-command").
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Optional description of what the command does.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CliCommandAttribute"/> class.
    /// </summary>
    /// <param name="name">The command name used in the CLI.</param>
    public CliCommandAttribute(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}
