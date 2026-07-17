using Cyrena.Attributes;
using Cyrena.Models;
using Cyrena.Screens.Contracts;
using Cyrena.Screens.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace Cyrena.Screens.Components.Shared;

public partial class ScreenShareTool : IDisposable
{
    [Inject] private IScreenInterop _screen { get; set; } = null!;
    [Inject] private ISnackbar _snackbar { get; set; } = null!;
    [Inject] private IJSRuntime _js { get; set; } = null!;

    private bool _supported;
    private bool _busy;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        _screen.Configure(_js);
        _screen.StateChanged += OnScreenStateChanged;
        _supported = await _screen.IsSupportedAsync();
        StateHasChanged();
    }

    private void OnScreenStateChanged(object? sender, EventArgs e)
        => InvokeAsync(StateHasChanged);

    private async Task ShareScreen()
    {
        if (_busy) return;
        _busy = true;
        try
        {
            // Result is intentionally NOT discarded — we surface it as
            // a snackbar so the user knows the share actually went
            // through. Without this, the picker closing is the only
            // visible signal, and a silent JS failure looks the same
            // as a successful share.
            ScreenOpResult result = await _screen.RequestShareAsync();
            if (result.Cancelled == true)
            {
                _snackbar.Add("Screen share cancelled.", Severity.Info);
            }
            else if (!result.Success)
            {
                _snackbar.Add($"Screen share failed: {result.Error ?? "unknown error"}", Severity.Error);
            }
        }
        finally
        {
            _busy = false;
        }
    }

    private async Task StopScreen()
    {
        if (_busy) return;
        _busy = true;
        try
        {
            var result = await _screen.StopAsync();
            if (!result.Success && result.Error is not null)
                _snackbar.Add($"Could not stop screen share: {result.Error}", Severity.Warning);
        }
        finally
        {
            _busy = false;
        }
    }

    public void Dispose()
    {
        // Unsubscribe defensively. _screen is [KernelInject] so it's
        // bound for the lifetime of the kernel; the handler reference
        // is per-render of this component. If the component is being
        // recreated (e.g. chat re-render), the old handler must not
        // leak into the new instance.
        if (_screen is not null)
            _screen.StateChanged -= OnScreenStateChanged;
        GC.SuppressFinalize(this);
    }
}
