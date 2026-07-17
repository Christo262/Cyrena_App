using Cyrena.Screens.Contracts;
using Cyrena.Screens.Models;
using Microsoft.JSInterop;

namespace Cyrena.Screens.Services;

/// <summary>
/// JS interop implementation for the screen-share feature. One
/// instance per kernel (one per chat). Holds the active source token
/// for the lifetime of the kernel so AI functions and UI components
/// can both call into it without juggling tokens. <see cref="Dispose"/>
/// releases the JS stream so the kernel teardown path is clean.
/// </summary>
internal sealed class ScreenInterop : IScreenInterop
{
    private const string JsNamespace = "Cyrena.ScreenShare";

    private IJSRuntime? _js;
    private bool _configured;
    private ScreenToken _active = ScreenToken.Empty;
    private string? _activeLabel;
    private string? _activeDisplaySurface;
    private int _activeWidth;
    private int _activeHeight;
    private bool _browserSupported;
    private bool _supportChecked;

    /// <inheritdoc />
    public event EventHandler? StateChanged;

    /// <inheritdoc />
    public bool IsActive => !_active.IsEmpty;

    /// <inheritdoc />
    public bool IsSupported => _js is not null && _browserSupported;

    /// <inheritdoc />
    public bool IsConfigured => _configured;

    /// <inheritdoc />
    public string? ActiveLabel => _activeLabel;

    /// <inheritdoc />
    public ScreenSource ActiveSource => ClassifySurface(_activeDisplaySurface);

    /// <inheritdoc />
    public int ActiveWidth => _activeWidth;

    /// <inheritdoc />
    public int ActiveHeight => _activeHeight;

    /// <inheritdoc />
    public void Configure(IJSRuntime jsRuntime)
    {
        ArgumentNullException.ThrowIfNull(jsRuntime);
        if (ReferenceEquals(_js, jsRuntime)) return;
        _js = jsRuntime;
        _configured = true;
        // Reset the support cache so the next requestShare call
        // re-asks the (possibly new) JS module whether getDisplayMedia
        // is available. Different runtimes, different reality.
        _supportChecked = false;
        _browserSupported = false;
    }

    /// <summary>
    /// Releases the JS runtime reference. Any active stream is
    /// released on the JS side first so the browser stops capturing
    /// immediately. Note: this runs when the kernel is torn down;
    /// the page may still be alive, in which case the JS Map is
    /// the only remaining handle. <see cref="ScreenShareTool"/>'s
    /// explicit <see cref="StopAsync"/> is the user-facing path.
    /// </summary>
    public void Dispose()
    {
        // Best-effort sync release. We can't await in Dispose, but
        // the browser will reclaim the stream on the next GC of
        // the JS module scope (or page unload). Forcing a fire-and-
        // forget release on the JS runtime gives us a chance to
        // stop the tracks right now, which stops the OS-level
        // capture indicator sooner.
        if (_js is not null && !_active.IsEmpty)
        {
            _ = FireAndForgetRelease(_js, _active);
        }
        _js = null;
        _configured = false;
        _active = ScreenToken.Empty;
        _activeLabel = null;
        _activeDisplaySurface = null;
        _activeWidth = 0;
        _activeHeight = 0;
    }

    private static async Task FireAndForgetRelease(IJSRuntime js, ScreenToken token)
    {
        try
        {
            // JS signature is releaseStream(dotNetRef, token). We pass
            // null for the dotNetRef slot because C# state is updated
            // by the caller, not by the JS callback. The trailing
            // null arguments are explicit so the wire shape stays
            // stable even if Blazor ever changes how it marshals
            // trailing args.
            await js.InvokeAsync<object>(
                $"{JsNamespace}.releaseStream",
                (object?)null,
                token.Value);
        }
        catch
        {
            // Disposal must not throw. JS is going away anyway.
        }
    }

    /// <inheritdoc />
    public async ValueTask<bool> IsSupportedAsync()
    {
        var js = _js;
        if (js is null) return false;
        try
        {
            _browserSupported = await js.InvokeAsync<bool>($"{JsNamespace}.isSupported");
            _supportChecked = true;
            return _browserSupported;
        }
        catch
        {
            _browserSupported = false;
            _supportChecked = true;
            return false;
        }
    }

    /// <inheritdoc />
    public async ValueTask<ScreenOpResult> RequestShareAsync(ScreenPickPreferences? preferences = null)
    {
        var js = _js;
        if (js is null || !_configured)
            return Failure("Screen share is not configured. The toolbar has not wired up the JS runtime yet.");

        if (!await EnsureSupportedAsync())
            return Failure("Screen sharing is not supported in this browser.");

        // Release any existing source first. The JS adapter returns
        // success even for unknown tokens, so this is safe.
        if (!_active.IsEmpty)
        {
            try
            {
                // JS signature is releaseStream(dotNetRef, token).
                // Pass null as the dotNetRef slot.
                await js.InvokeAsync<object>(
                    $"{JsNamespace}.releaseStream",
                    (object?)null,
                    _active.Value);
            }
            catch { /* proceed to pick anyway — best effort */ }
            ClearActive();
        }

        // Pass preferences as an anonymous object (or null) so
        // the wire shape is stable: ALWAYS one object arg, never
        // missing, never shape-changed. JS can destructure
        // { displaySurface, audio } reliably.
        //
        // CRITICAL: JS signature is startShareSync / requestShare(dotNetRef, prefs).
        // The first arg is the DotNetObjectReference slot. C# state
        // is updated by the return value, not by JS callbacks, so
        // we pass null here.
        var prefs = preferences is null
            ? null
            : new { displaySurface = preferences.DisplaySurface, audio = preferences.Audio };

        ScreenOpResult result;
        try
        {
            // Two-step call when the runtime supports synchronous interop
            // (Blazor WebAssembly, Blazor Hybrid / Photino / WebView2).
            // The sync start fires getDisplayMedia() inside the same task
            // as the user-activation event from the Blazor OnClick handler,
            // which is what Chromium requires -- await would cross a
            // microtask boundary and the user-activation would expire,
            // producing the "must be called from a user gesture handler"
            // error.
            //
            // On runtimes that don't support IJSInProcessRuntime (Blazor
            // Server, SignalR-based hosts), we fall back to the async
            // requestShare wrapper. The user-gesture rule is enforced
            // loosely on Blazor Server, so the fallback is fine there.
            if (js is IJSInProcessRuntime inProc)
            {
                result = StartShareSync(inProc, prefs);
                if (result.Success && !string.IsNullOrEmpty(result.Token))
                {
                    // Picker is up. Now cross the await boundary to wait
                    // for the user to actually pick. Hard 60s timeout
                    // guards against the OS picker never resolving (e.g.
                    // the user closed the underlying WebView window).
                    var token = result.Token;
                    result = await inProc.InvokeAsync<ScreenOpResult>(
                        $"{JsNamespace}.awaitPendingWithTimeout",
                        token,
                        60_000);
                }
            }
            else
            {
                result = await js.InvokeAsync<ScreenOpResult>(
                    $"{JsNamespace}.requestShare",
                    (object?)null,
                    prefs);
            }
        }
        catch (JSException ex)
        {
            return Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Failure(ex.Message);
        }

        // IMPORTANT: the token we store is the token JS returned.
        // If JS stored the stream under a DIFFERENT key, this will
        // not match. That's a JS bug to fix on the JS side. We trust
        // the wire here.
        if (result.Success && !string.IsNullOrEmpty(result.Token))
        {
            _active = new ScreenToken(result.Token);
            _activeLabel = result.Label;
            _activeDisplaySurface = result.DisplaySurface;
            _activeWidth = result.Width ?? 0;
            _activeHeight = result.Height ?? 0;
            RaiseStateChanged();
        }
        else if (!result.Success)
        {
            // Make sure any stale UI state is cleared on failure.
            // The user might have picked a source, then dismissed,
            // or the picker might have thrown. Either way, IsActive
            // must be false so the toolbar flips back.
            ClearActive();
            RaiseStateChanged();
        }

        return result;
    }

    /// <summary>
    /// Synchronous start of the screen-share picker. Calls the JS
    /// startShareSync entry point via the in-process JS runtime so the
    /// getDisplayMedia() call lands in the same task as the originating
    /// Blazor OnClick handler. Without this, C#'s await on
    /// InvokeAsync yields the dispatcher, the user-activation token
    /// expires, and Chromium rejects the picker with "must be called
    /// from a user gesture handler". This is the failure mode
    /// Photino/WebView2 on Linux produces.
    ///
    /// Returns the JS sync result envelope (success + token, or
    /// success=false with an error). The caller is expected to follow
    /// up with awaitPendingWithTimeout to retrieve the final result.
    /// </summary>
    private static ScreenOpResult StartShareSync(IJSInProcessRuntime inProc, object? prefs)
    {
        // JS signature is startShareSync(dotNetRef, prefs). Pass
        // null for the dotNetRef slot -- C# state is updated from
        // the return value path, not via JS callbacks. The IJSInProcessRuntime
        // overload that takes no dotNetRef slot would be ideal but the
        // module's function is parameterized with it, so we pass null.
        return inProc.Invoke<ScreenOpResult>(
            $"{JsNamespace}.startShareSync",
            (object?)null,
            prefs);
    }

    /// <inheritdoc />
    public async ValueTask<ScreenOpResult> CaptureAsync()
    {
        var js = _js;
        if (js is null)
            return Failure("Screen share is not configured.");
        if (_active.IsEmpty)
            return Failure("No active screen source. Call RequestShareAsync first.");

        ScreenOpResult result;
        try
        {
            // JS signature is captureStream(dotNetRef, token). Pass
            // null as the dotNetRef slot. Without the null here the
            // token arg lands in the dotNetRef slot and the actual
            // token becomes undefined — Map.get(undefined) returns
            // undefined, and JS reports "No active stream for the
            // given token."
            result = await js.InvokeAsync<ScreenOpResult>(
                $"{JsNamespace}.captureStream",
                (object?)null,
                _active.Value);
        }
        catch (JSException ex)
        {
            return Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Failure(ex.Message);
        }

        // The source can be revoked between request and capture. When
        // that happens, JS reports SourceLost=true. We clear local
        // state and notify so the UI updates to "not sharing".
        if (result.SourceLost == true)
        {
            ClearActive();
            RaiseStateChanged();
        }

        return result;
    }

    /// <inheritdoc />
    public async ValueTask<ScreenOpResult> StopAsync()
    {
        var js = _js;
        if (js is null || _active.IsEmpty)
            return Failure("No active screen source.");
        try
        {
            // JS signature is releaseStream(dotNetRef, token). Pass
            // null as the dotNetRef slot. We update C# state from
            // the return value path below, not via the JS callback.
            var result = await js.InvokeAsync<ScreenOpResult>(
                $"{JsNamespace}.releaseStream",
                (object?)null,
                _active.Value);
            ClearActive();
            RaiseStateChanged();
            return result;
        }
        catch (JSException ex)
        {
            return Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Failure(ex.Message);
        }
    }

    private async ValueTask<bool> EnsureSupportedAsync()
    {
        if (_js is null) return false;
        if (!_supportChecked) await IsSupportedAsync();
        return _browserSupported;
    }

    private void ClearActive()
    {
        _active = ScreenToken.Empty;
        _activeLabel = null;
        _activeDisplaySurface = null;
        _activeWidth = 0;
        _activeHeight = 0;
    }

    private void RaiseStateChanged()
    {
        try
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
        catch
        {
            // A bad subscriber must not break the interop. The UI
            // will still see the new state on the next render.
        }
    }

    private static ScreenSource ClassifySurface(string? surface) => surface switch
    {
        null => ScreenSource.None,
        "" => ScreenSource.Unknown,
        "monitor" => ScreenSource.Monitor,
        "window" => ScreenSource.Window,
        "browser" => ScreenSource.Browser,
        _ => ScreenSource.Unknown
    };

    private static ScreenOpResult Failure(string message) => new()
    {
        Success = false,
        Error = message
    };
}
