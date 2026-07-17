using System.Text.Json.Serialization;

namespace Cyrena.Screens.Models;

/// <summary>
/// Snapshot of a single currently-held stream, returned by
/// <see cref="Contracts.IScreenInterop.ListStreamsAsync"/>. Used to
/// rebuild Blazor state after a Blazor Server reconnect.
/// </summary>
public sealed class ScreenStreamInfo
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("displaySurface")]
    public string DisplaySurface { get; set; } = "unknown";

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("readyState")]
    public string ReadyState { get; set; } = "live";
}
