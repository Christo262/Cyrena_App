using Cyrena.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Cyrena.Extensa.Models;

/// <summary>
/// Represents a plugin distribution server configuration.
/// </summary>
public class PluginServer : Entity
{
    /// <summary>
    /// Display name of the server.
    /// </summary>
    /// 
    [Required]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Base URL of the plugin server (e.g., "https://plugins.example.com").
    /// </summary>
    /// 
    [Required]
    [Url]
    public string BaseUrl { get; set; } = default!;

    /// <summary>
    /// Optional application ID for this server.
    /// </summary>
    /// 
    [Required]
    public string ApplicationId { get; set; } = default!;

    /// <summary>
    /// Indicates whether this server is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Optional priority for ordering servers (lower = higher priority).
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// When this server was added.
    /// </summary>
    public DateTimeOffset AddedAt { get; set; } = DateTimeOffset.UtcNow;

    [JsonIgnore]
    public bool IsDefault { get; set; }
}
