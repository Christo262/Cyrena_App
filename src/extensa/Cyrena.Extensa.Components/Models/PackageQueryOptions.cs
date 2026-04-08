namespace Cyrena.Extensa.Models;

/// <summary>
/// Options for querying packages from a plugin server.
/// </summary>
public class PackageQueryOptions
{
    /// <summary>
    /// Target operating system filter.
    /// Supported values: "win", "mac", "linux", "android".
    /// </summary>
    public string? Os { get; set; }

    /// <summary>
    /// Target architecture filter.
    /// Supported values: "x64", "arm64", "x86", "armv7".
    /// </summary>
    public string? Arch { get; set; }

    /// <summary>
    /// Specific version to retrieve.
    /// </summary>
    public string? Version { get; set; }
}
