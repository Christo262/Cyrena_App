using Cyrena.Models;

namespace Cyrena.Extensa.Models;

/// <summary>
/// Represents a package retrieved from a plugin distribution server.
/// </summary>
public class Package : Entity
{
    /// <summary>
    /// Display name of the package.
    /// </summary>
    public string Title { get; set; } = default!;

    /// <summary>
    /// Description of the package.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The server this package was retrieved from.
    /// </summary>
    public string? ServerId { get; set; }

    /// <summary>
    /// Available versions for this package.
    /// </summary>
    public List<PackageVersion> Versions { get; set; } = [];

    public string[] SupportedOperatingSystems { get; set; } = [];
    public string[] SupportedArchitectures { get; set;  } = [];
    public bool HasIcon { get; set; }
    public string? IconUrl { get; set; }
    public string? WebPageLink { get; set; }
}
