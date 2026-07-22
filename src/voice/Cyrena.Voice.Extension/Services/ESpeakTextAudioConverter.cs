using System.Diagnostics;
using System.Text;
using Cyrena.Contracts;
using Cyrena.Voice.Contracts;
using Cyrena.Voice.Models;
using Cyrena.Voice.Options;

namespace Cyrena.Voice.Services;

/// <summary>
/// Text-to-speech converter backed by the eSpeak command-line synthesizer.
/// Spawns <c>espeak "text"</c> with the configured voice/pace/pitch/volume
/// flags, lets eSpeak play directly to the default audio device, and
/// returns an empty <see cref="WebViewAudioArtifact"/> as a marker (the
/// same pattern used by <see cref="WebViewTextAudioConverter"/>).
/// </summary>
internal class ESpeakTextAudioConverter : ITextAudioConverter
{
    public const string Key = "espeak.tts";

    private readonly ISettingsService _settings;

    // eSpeak plays directly to the host's default audio device. If two
    // ConvertAsync calls run concurrently, the second eSpeak process opens
    // a fresh audio stream while the first is still flushing, which
    // manifests as clipped/cut-off audio and overlapping utterances
    // (especially with PulseAudio/PipeWire on Linux). Serialise every
    // call through this gate so one eSpeak invocation fully exits —
    // and therefore fully releases the audio device — before the next
    // starts. The WebView converter is unaffected because it doesn't
    // take this gate.
    private readonly SemaphoreSlim _gate = new(1, 1);

    public ESpeakTextAudioConverter(ISettingsService settings)
    {
        _settings = settings;
    }

    public bool IsInitialized { get; private set; }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        // eSpeak is a stateless external process; nothing to warm up.
        IsInitialized = true;
        return Task.CompletedTask;
    }

    public async Task DeinitializeAsync(CancellationToken cancellationToken = default)
    {
        // Nothing held; just flip the flag for symmetry with the WebView impl.
        await Task.CompletedTask;
        IsInitialized = false;
    }

    public void Dispose()
    {
        IsInitialized = false;
        _gate.Dispose();
    }

    public async Task<AudioArtifact> ConvertAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new WebViewAudioArtifact();

        // Block the next caller until the current eSpeak process has
        // fully exited and released the audio device. WaitAsync honours
        // the same token used to kill the in-flight process, so a cancel
        // here doesn't deadlock the queue.
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var options = _settings.Read<ESpeakOptions>(ESpeakOptions.Key) ?? new ESpeakOptions();
            var startInfo = new ProcessStartInfo
            {
                FileName = string.IsNullOrWhiteSpace(options.BinaryPath) ? "espeak" : options.BinaryPath,
                Arguments = BuildArguments(options, text),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                StandardErrorEncoding = Encoding.UTF8,
            };

            using var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
            var stderr = new StringBuilder();
            process.ErrorDataReceived += (_, e) => { if (e.Data is not null) stderr.AppendLine(e.Data); };

            using var killReg = cancellationToken.Register(() =>
            {
                try { if (!process.HasExited) process.Kill(entireProcessTree: true); }
                catch { /* race with natural exit */ }
            });

            try
            {
                if (!process.Start())
                    throw new InvalidOperationException("Failed to start espeak process.");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                throw new InvalidOperationException(
                    $"espeak binary not found (looked for '{startInfo.FileName}'). " +
                    "Set ESpeakOptions.BinaryPath or install espeak on the system PATH.",
                    ex);
            }

            process.BeginErrorReadLine();
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                var err = stderr.ToString().Trim();
                throw new InvalidOperationException(
                    $"espeak exited with code {process.ExitCode}. {err}");
            }

            return new WebViewAudioArtifact();
        }
        finally
        {
            _gate.Release();
        }
    }

    private static string BuildArguments(ESpeakOptions options, string text)
    {
        var args = new StringBuilder();
        args.Append("-v ").Append(Quote(options.Voice)).Append(' ');
        args.Append("-s ").Append(options.Speed).Append(' ');
        args.Append("-p ").Append(options.Pitch).Append(' ');
        args.Append("-a ").Append(options.Amplitude).Append(' ');
        if (options.WordGap > 0)
            args.Append("-g ").Append(options.WordGap).Append(' ');
        // Wrap text in single quotes; escape embedded single quotes
        // so a stray apostrophe can't break out of the quoted region.
        args.Append('\'').Append(text.Replace("'", @"'\''")).Append('\'');
        return args.ToString();
    }

    private static string Quote(string value) =>
        value.Contains(' ') ? $"\"{value}\"" : value;
}
