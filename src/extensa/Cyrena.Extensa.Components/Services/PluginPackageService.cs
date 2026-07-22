using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cyrena.Extensa.Contracts;
using Cyrena.Extensa.Models;
using Cyrena.Extensa.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cyrena.Extensa.Services;

/// <summary>
/// Service for querying and downloading packages from plugin servers.
/// </summary>
internal class PluginPackageService : IPluginPackageService
{
    private readonly IPluginServerService _serverService;
    private readonly ILogger<PluginPackageService> _logger;
    private readonly PluginServerOptions _options;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public PluginPackageService(
        IPluginServerService serverService,
        ILogger<PluginPackageService> logger,
        IOptions<PluginServerOptions> options)
    {
        _serverService = serverService;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<Package>> GetPackagesAsync(
        PluginServer server,
        PackageQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var url = BuildPackagesUrl(new Uri(server.BaseUrl), server.ApplicationId, options);
        _logger.LogDebug("Fetching packages from {Url}", url);

        using var httpClient = new HttpClient();
        var packagesResponse = await httpClient.GetFromJsonAsync<PackageListResponse>(url, JsonOptions, cancellationToken);

        if (packagesResponse == null)
            return [];

        var packages = packagesResponse
            .Select(p => MapPackage(p, server))
            .ToList();

        _logger.LogInformation("Retrieved {Count} packages from server {ServerId}", packages.Count, server.Id);
        return packages;
    }

    public async Task<Package?> GetPackageAsync(
        PluginServer server,
        string packageId,
        PackageQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var url = BuildPackageUrl(new Uri(server.BaseUrl), server.ApplicationId, packageId, options);
        _logger.LogDebug("Fetching package {PackageId} from {Url}", packageId, url);

        using var httpClient = new HttpClient();

        try
        {
            var packageResponse = await httpClient.GetFromJsonAsync<Package>(url, JsonOptions, cancellationToken);

            return packageResponse;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Package {PackageId} not found on server {ServerId}", packageId, server.Id);
            return null;
        }
    }

    public async Task<PackageDownloadResult> DownloadPackageAsync(
        PluginServer server,
        string packageId,
        PackageQueryOptions? options = null,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var downloadUrl = BuildDownloadUrl(new Uri(server.BaseUrl), server.ApplicationId, packageId, options?.Version);
        _logger.LogDebug("Downloading package {PackageId} from {Url}", packageId, downloadUrl);

        using var httpClient = new HttpClient();

        using var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var contentLength = response.Content.Headers.ContentLength ?? -1L;
        var fileName = $"{packageId}.zip";

        // Try to get hash from custom header
        var contentHash = response.Headers.TryGetValues("X-Content-Hash", out var hashValues) 
            ? hashValues.FirstOrDefault() 
            : null;

        var version = options?.Version;

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var memoryStream = new MemoryStream();

        if (contentLength > 0)
        {
            var buffer = new byte[81920];
            long totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await memoryStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                totalBytesRead += bytesRead;

                if (contentLength > 0)
                {
                    progress?.Report(((double)totalBytesRead / contentLength) * 100);
                }
            }
        }
        else
        {
            await stream.CopyToAsync(memoryStream, cancellationToken);
        }

        memoryStream.Position = 0;

        _logger.LogInformation("Downloaded package {PackageId} ({Size} bytes) from server {ServerId}", 
            packageId, memoryStream.Length, server.Id);

        return new PackageDownloadResult
        {
            Stream = memoryStream,
            FileName = fileName,
            ContentLength = memoryStream.Length,
            ContentHash = contentHash,
            Version = version
        };
    }

    public async Task<byte[]?> GetPackageIconAsync(
        PluginServer server,
        string packageId,
        CancellationToken cancellationToken = default)
    {
        var url = $"{server.BaseUrl}api/applications/{server.ApplicationId}/packages/{packageId}/icon";
        _logger.LogDebug("Fetching icon for package {PackageId} from {Url}", packageId, url);

        using var httpClient = new HttpClient();

        try
        {
            return await httpClient.GetByteArrayAsync(url, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Icon not found for package {PackageId} on server {ServerId}", packageId, server.Id);
            return null;
        }
    }

    public async Task<IReadOnlyList<PackageSearchResult>> SearchPackagesAsync(
        string applicationId,
        PackageQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var enabledServers = await _serverService.GetEnabledServers();
        var results = new List<PackageSearchResult>();

        _logger.LogInformation("Searching packages across {ServerCount} enabled servers", enabledServers.Count());

        var tasks = enabledServers.Select(async server =>
        {
            try
            {
                var packages = await GetPackagesAsync(server, options, cancellationToken);
                return (Server: server, Packages: packages, Error: null as Exception);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to search packages on server {ServerId}", server.Id);
                return (Server: server, Packages: Array.Empty<Package>(), Error: ex);
            }
        });

        var taskResults = await Task.WhenAll(tasks);

        foreach (var (server, packages, error) in taskResults)
        {
            if (error == null)
            {
                foreach (var package in packages)
                {
                    results.Add(new PackageSearchResult
                    {
                        Package = package,
                        Server = server
                    });
                }
            }
        }

        return results;
    }

    private string BuildPackagesUrl(Uri baseUrl, string applicationId, PackageQueryOptions? options = null)
    {
        var url = $"{baseUrl}api/applications/{applicationId}/packages";

        var queryParams = new List<string>();

        var os = options?.Os ?? _options.DetectedOs;
        if (!string.IsNullOrWhiteSpace(os))
            queryParams.Add($"os={Uri.EscapeDataString(os)}");

        var arch = options?.Arch ?? _options.DetectedArch;
        if (!string.IsNullOrWhiteSpace(arch))
            queryParams.Add($"arch={Uri.EscapeDataString(arch)}");

        if (queryParams.Count > 0)
            url += "?" + string.Join("&", queryParams);

        return url;
    }

    private string BuildPackageUrl(Uri baseUrl, string applicationId, string packageId, PackageQueryOptions? options)
    {
        var url = $"{baseUrl}api/applications/{applicationId}/packages/{packageId}";

        var queryParams = new List<string>();

        var os = options?.Os ?? _options.DetectedOs;
        if (!string.IsNullOrWhiteSpace(os))
            queryParams.Add($"os={Uri.EscapeDataString(os)}");

        var arch = options?.Arch ?? _options.DetectedArch;
        if (!string.IsNullOrWhiteSpace(arch))
            queryParams.Add($"arch={Uri.EscapeDataString(arch)}");

        if (queryParams.Count > 0)
            url += "?" + string.Join("&", queryParams);

        return url;
    }

    private string BuildDownloadUrl(Uri baseUrl, string applicationId, string packageId, string? version = null)
    {
        var url = $"{baseUrl}api/applications/{applicationId}/packages/{packageId}/download";

        if (!string.IsNullOrWhiteSpace(version))
            url += $"?version={Uri.EscapeDataString(version)}";

        return url;
    }

    private static Package MapPackage(Package item, PluginServer server)
    {
        item.ServerId = server.Id;
        if (item.HasIcon)
            item.IconUrl = $"{server.BaseUrl.TrimEnd("/")}/api/applications/{server.ApplicationId}/packages/{item.Id}/icon";
        return item;
    }
}

// API Response DTOs

internal class PackageListResponse : List<Package>
{
}