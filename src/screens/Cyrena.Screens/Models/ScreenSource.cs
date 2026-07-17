using System.Text.Json.Serialization;

namespace Cyrena.Screens.Models;

/// <summary>
/// Classified type of a screen source, returned by the browser's
/// <c>displaySurface</c> hint and (when missing) inferred from the
/// track label by the JS adapter.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ScreenSource
{
    /// <summary>No active source — the stream is not open or was released.</summary>
    None = 0,

    /// <summary>Entire physical display / monitor.</summary>
    Monitor = 1,

    /// <summary>A specific application window.</summary>
    Window = 2,

    /// <summary>A browser tab.</summary>
    Browser = 3,

    /// <summary>Surface could not be classified (label did not match any heuristic).</summary>
    Unknown = 4
}
