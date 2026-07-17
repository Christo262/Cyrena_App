using Cyrena.Screens.Models;
using Microsoft.JSInterop;

namespace Cyrena.Screens.Contracts;

/// <summary>
/// JS interop contract for the screen-share feature, kernel-scoped
/// (one instance per chat). The implementation holds the active
/// <see cref="ScreenToken"/> for the lifetime of the kernel, so AI
/// function calls (e.g. <c>Functions.ScreenshotAsync</c>) and UI
/// components share the same source without passing tokens around.
///
/// State model:
/// <list type="bullet">
///   <item><description>The implementation holds the active token; consumers never juggle it.</description></item>
///   <item><description><see cref="RequestShareAsync"/> replaces any current source — one source per chat.</description></item>
///   <item><description><see cref="CaptureAsync"/> operates on whatever is currently held.</description></item>
///   <item><description><see cref="StopAsync"/> releases the current source without picking a new one.</description></item>
///   <item><description><see cref="StateChanged"/> fires on every change so UI components can re-render.</description></item>
/// </list>
///
/// Wiring:
/// <list type="number">
///   <item><description>The extension registers this as a singleton in the kernel's <c>IServiceCollection</c>.</description></item>
///   <item><description>The <c>ScreenShareTool</c> component injects the service and calls <see cref="Configure"/> with its <see cref="IJSRuntime"/>.</description></item>
///   <item><description>The <c>Functions</c> plugin injects the same service to call <see cref="CaptureAsync"/>.</description></item>
///   <item><description>When the kernel is torn down, <see cref="Dispose"/> runs and releases the JS stream.</description></item>
/// </list>
/// </summary>
public interface IScreenInterop : IDisposable
{
    /// <summary>
    /// Fires whenever the active state changes: share started, source
    /// replaced, source ended (user revoked), or stopped. UI components
    /// subscribe in <c>OnInitialized</c> and unsubscribe in <c>Dispose</c>.
    /// </summary>
    event EventHandler? StateChanged;

    /// <summary>
    /// Bind the service to a <see cref="IJSRuntime"/>. Called by the
    /// <c>ScreenShareTool</c> component after the first render. The
    /// runtime is only available inside an active component lifetime
    /// and the kernel's services are constructed before any component
    /// renders, so the runtime has to be passed in explicitly. Safe
    /// to call multiple times; the most recent runtime wins.
    /// </summary>
    void Configure(IJSRuntime jsRuntime);

    /// <summary>
    /// <c>true</c> when the service is bound to a runtime, the browser
    /// supports <c>getDisplayMedia</c>, and a source is currently held.
    /// UI components bind to this for their "is sharing" state.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// <c>true</c> when <see cref="Configure"/> has been called AND
    /// the browser supports <c>getDisplayMedia</c>. False before
    /// configure, false in unsupported browsers, true otherwise.
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// <c>true</c> when <see cref="Configure"/> has bound a runtime
    /// to this instance. Independent of <see cref="IsSupported"/>:
    /// a runtime can be configured but the browser may still not
    /// support <c>getDisplayMedia</c>. Useful for toolbars that want
    /// to show a "preparing..." state before support is checked.
    /// </summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Display label of the active source, e.g. <c>"Display 2"</c> or
    /// <c>"Window: Visual Studio Code"</c>. <c>null</c> when not active.
    /// </summary>
    string? ActiveLabel { get; }

    /// <summary>
    /// Classified source type. <see cref="ScreenSource.None"/> when
    /// not active. Used by the UI for an icon and by the AI for
    /// reasoning about what the capture will contain.
    /// </summary>
    ScreenSource ActiveSource { get; }

    /// <summary>
    /// Width in pixels of the active source, or 0 when not active.
    /// </summary>
    int ActiveWidth { get; }

    /// <summary>
    /// Height in pixels of the active source, or 0 when not active.
    /// </summary>
    int ActiveHeight { get; }

    /// <summary>
    /// Show the OS picker. If a source is already held, it is released
    /// first (no callback fires for that — the new pick replaces it).
    /// The <paramref name="preferences"/> are passed through to the
    /// browser as hints; they don't filter the picker.
    /// </summary>
    /// <returns>
    /// On success: <see cref="ScreenOpResult.Success"/> is true. The
    /// new token is held internally; <see cref="StateChanged"/> has
    /// fired. On cancel: <see cref="ScreenOpResult.Cancelled"/> is
    /// true; the previous source (if any) is also gone. On other
    /// failure: <see cref="ScreenOpResult.Error"/> is set; the
    /// previous source (if any) is also gone.
    /// </returns>
    ValueTask<ScreenOpResult> RequestShareAsync(ScreenPickPreferences? preferences = null);

    /// <summary>
    /// Capture one frame from the currently-held source. The stream
    /// stays alive after capture.
    /// </summary>
    /// <returns>
    /// On success: <see cref="ScreenOpResult.Success"/> is true and
    /// <see cref="ScreenOpResult.DataUrl"/> is a base64 PNG ready to
    /// feed into the file-paste pipeline. If the source has ended
    /// (user revoked via browser chrome): <see cref="ScreenOpResult.SourceLost"/>
    /// is true; <see cref="IsActive"/> is now false and
    /// <see cref="StateChanged"/> has fired.
    /// </returns>
    ValueTask<ScreenOpResult> CaptureAsync();

    /// <summary>
    /// Release the current source. Safe when nothing is active.
    /// Fires <see cref="StateChanged"/> if a source was released.
    /// </summary>
    /// <returns>
    /// On success: <see cref="ScreenOpResult.Success"/> is true. When
    /// no source is active: <see cref="ScreenOpResult.Success"/> is
    /// false with a descriptive <see cref="ScreenOpResult.Error"/>.
    /// </returns>
    ValueTask<ScreenOpResult> StopAsync();

    ValueTask<bool> IsSupportedAsync();
}
