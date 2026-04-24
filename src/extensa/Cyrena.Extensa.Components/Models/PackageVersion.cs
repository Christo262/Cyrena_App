namespace Cyrena.Extensa.Models;

/// <summary>
/// Represents a version of a package.
/// </summary>
public class PackageVersion
{
    /// <summary>
    /// Semantic version string (e.g., "1.2.3").
    /// </summary>
    public Version Version { get; set; } = new Version(0,0,0);

    /// <summary>
    /// Optional release notes or changelog.
    /// </summary>
    public string? ReleaseNotes { get; set; }

    /// <summary>
    /// Release date of this version.
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// SHA-256 content hash for verification (format: "sha256:...").
    /// </summary>
    public string? ContentHash { get; set; }

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long? SizeBytes { get; set; }
}
