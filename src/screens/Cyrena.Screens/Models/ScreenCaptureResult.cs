namespace Cyrena.Screens.Models;

/// <summary>
/// Strongly-typed view of <see cref="ScreenOpResult"/> for the
/// capture-specific call. The JS adapter returns a
/// <c>dataUrl</c> / <c>fileName</c> / <c>mimeType</c> / <c>size</c>
/// bundle which this projects onto the existing
/// <c>OnFilePasted</c> contract used by the rest of the app.
/// </summary>
public sealed class ScreenCaptureResult
{
    public ScreenToken Token { get; set; }
    public string? DataUrl { get; set; }
    public string? FileName { get; set; }
    public string? MimeType { get; set; }
    public long? Size { get; set; }
    public string? Label { get; set; }
    public ScreenSource DisplaySurface { get; set; } = ScreenSource.Unknown;
}
