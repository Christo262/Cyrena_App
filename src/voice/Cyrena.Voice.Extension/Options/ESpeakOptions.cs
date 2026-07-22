namespace Cyrena.Voice.Options;

/// <summary>
/// Configuration for <see cref="Cyrena.Voice.Services.ESpeakTextAudioConverter"/>.
/// Persisted via <c>ISettingsService</c> under <see cref="Key"/>.
/// All properties are espeak command-line flag defaults. Values outside the
/// documented eSpeak ranges will cause the underlying command to fail.
/// </summary>
public class ESpeakOptions
{
    public const string Key = "espeak.tts";

    /// <summary>
    /// Path to the espeak binary. If null or empty, the converter resolves
    /// <c>espeak</c> from the system PATH via the OS process launcher.
    /// </summary>
    public string? BinaryPath { get; set; }

    /// <summary>
    /// Voice name to use (e.g. <c>en</c>, <c>en+f3</c>, <c>de</c>).
    /// See <c>espeak --voices</c>. Defaults to <c>en</c>.
    /// </summary>
    public string Voice { get; set; } = "en";

    /// <summary>
    /// Speed in words per minute. eSpeak default is 175; valid range is
    /// roughly 80–500. Mirrors espeak's <c>-s</c> flag.
    /// </summary>
    public int Speed { get; set; } = 175;

    /// <summary>
    /// Pitch adjustment. eSpeak default is 50; valid range is 0–99.
    /// Mirrors espeak's <c>-p</c> flag.
    /// </summary>
    public int Pitch { get; set; } = 50;

    /// <summary>
    /// Volume in the range 0–200. eSpeak default is 100.
    /// Mirrors espeak's <c>-a</c> flag.
    /// </summary>
    public int Amplitude { get; set; } = 100;

    /// <summary>
    /// Pause between words in 10 ms units. eSpeak default is 0.
    /// Mirrors espeak's <c>-g</c> flag.
    /// </summary>
    public int WordGap { get; set; } = 0;
}
