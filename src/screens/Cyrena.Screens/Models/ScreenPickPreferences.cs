using System.Text.Json.Serialization;

namespace Cyrena.Screens.Models;

/// <summary>
/// Caller hints for the OS picker. <see cref="DisplaySurface"/> is a hint
/// — browsers may ignore it, the picker still shows everything.
/// <see cref="Audio"/> defaults to false: most screenshot flows don't
/// need system audio in the same capture.
/// </summary>
public sealed class ScreenPickPreferences
{
    /// <summary>
    /// Optional hint: <c>"monitor"</c>, <c>"window"</c>, or <c>"browser"</c>.
    /// Some browsers pre-select the indicated category in the OS picker.
    /// </summary>
    [JsonPropertyName("displaySurface")]
    public string? DisplaySurface { get; set; }

    /// <summary>
    /// When <c>true</c>, the picker also includes a system-audio track
    /// (when the source supports it). Default is <c>false</c>.
    /// </summary>
    [JsonPropertyName("audio")]
    public bool Audio { get; set; }
}
