using System.Text.Json.Serialization;

namespace Cyrena.Screens.Models;

/// <summary>
/// Track settings echoed back by the JS adapter on a successful
/// pick. Useful for the Blazor layer to display "Sharing:
/// 2560x1440 — Display 2" without doing further work.
/// </summary>
public sealed class ScreenSettingsPayload
{
    [JsonPropertyName("width")]
    public int? Width { get; set; }

    [JsonPropertyName("height")]
    public int? Height { get; set; }

    [JsonPropertyName("frameRate")]
    public double? FrameRate { get; set; }
}
