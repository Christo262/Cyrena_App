using System;
using System.Runtime.InteropServices;

namespace Cyrena.Extensa.Options;

/// <summary>
/// Configuration options for plugin server management.
/// </summary>
public class PluginServerOptions
{
    /// <summary>
    /// Auto-detected operating system (win, mac, linux, android).
    /// </summary>
    public string DetectedOs { get; }

    /// <summary>
    /// Auto-detected architecture (x64, arm64, x86, armv7).
    /// </summary>
    public string DetectedArch { get; }

    /// <summary>
    /// Default servers to include on startup.
    /// </summary>
    public List<DefaultServer> DefaultServers { get; set; } = [];

    /// <summary>
    /// HTTP request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Enable automatic retry on transient failures.
    /// </summary>
    public bool EnableRetry { get; set; } = true;

    /// <summary>
    /// Maximum number of retries.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Initializes detection using current runtime.
    /// </summary>
    public PluginServerOptions()
    {
        DetectedOs = DetectOs();
        DetectedArch = DetectArch();
    }

    private static string DetectOs()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return "win";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return "mac";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return "linux";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD)) return "linux";
        return "win"; // fallback
    }

    private static string DetectArch()
    {
        return RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.Arm64 => "arm64",
            Architecture.X86 => "x86",
            Architecture.Arm => "armv7",
            _ => "x64" // fallback
        };
    }
}

/// <summary>
/// Represents a default server configuration.
/// </summary>
public class DefaultServer
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string BaseUrl { get; set; }
    public required string ApplicationId { get; set; }
    public int Priority { get; set; }
}
