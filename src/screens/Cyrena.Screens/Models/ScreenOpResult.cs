using System.Text.Json.Serialization;

namespace Cyrena.Screens.Models;

/// <summary>
/// Result envelope returned by every JS adapter call. The contract
/// uses one shape for everything: <see cref="Success"/> is the only
/// field the consumer should switch on. <see cref="Cancelled"/>,
/// <see cref="SourceLost"/>, and <see cref="Error"/> describe the
/// failure mode. <see cref="DataUrl"/>, <see cref="FileName"/>,
/// <see cref="MimeType"/>, and <see cref="Size"/> are populated only
/// on a successful capture.
/// </summary>
public sealed class ScreenOpResult
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("token")]
    public string? Token { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("displaySurface")]
    public string? DisplaySurface { get; set; }

    [JsonPropertyName("width")]
    public int? Width { get; set; }

    [JsonPropertyName("height")]
    public int? Height { get; set; }

    [JsonPropertyName("settings")]
    public ScreenSettingsPayload? Settings { get; set; }

    [JsonPropertyName("cancelled")]
    public bool? Cancelled { get; set; }

    [JsonPropertyName("sourceLost")]
    public bool? SourceLost { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("dataUrl")]
    public string? DataUrl { get; set; }

    [JsonPropertyName("fileName")]
    public string? FileName { get; set; }

    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }

    [JsonPropertyName("size")]
    public long? Size { get; set; }
}
