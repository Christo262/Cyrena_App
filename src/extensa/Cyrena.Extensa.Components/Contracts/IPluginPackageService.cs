using Cyrena.Extensa.Models;

namespace Cyrena.Extensa.Contracts;

/// <summary>
/// Service for querying and downloading packages from plugin servers.
/// </summary>
public interface IPluginPackageService
{
    /// <summary>
    /// Lists all packages from a specific server and application.
    /// </summary>
    Task<IReadOnlyList<Package>> GetPackagesAsync(
        PluginServer server,
        PackageQueryOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific package by ID from a server.
    /// </summary>
    Task<Package?> GetPackageAsync(
        PluginServer server,
        string packageId,
        PackageQueryOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a package as a stream.
    /// </summary>
    Task<PackageDownloadResult> DownloadPackageAsync(
        PluginServer server,
        string packageId,
        PackageQueryOptions? options = null,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the icon for a package.
    /// </summary>
    Task<byte[]?> GetPackageIconAsync(
        PluginServer server,
        string packageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for a package across all enabled servers.
    /// </summary>
    Task<IReadOnlyList<PackageSearchResult>> SearchPackagesAsync(
        string applicationId,
        PackageQueryOptions? options = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a package search across servers.
/// </summary>
public class PackageSearchResult
{
    public required Package Package { get; init; }
    public required PluginServer Server { get; init; }
}
