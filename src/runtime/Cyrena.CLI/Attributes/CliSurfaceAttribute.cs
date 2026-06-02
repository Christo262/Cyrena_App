namespace Cyrena.CLI.Attributes;

/// <summary>
/// Marks a class as a CLI command surface, indicating it contains CLI command methods.
/// Only classes marked with this attribute will be discovered and registered by the CLI service.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class CliSurfaceAttribute : Attribute
{
    /// <summary>
    /// Optional description of the command surface for documentation purposes.
    /// </summary>
    public string? Description { get; set; }
}
