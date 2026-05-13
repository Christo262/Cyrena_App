using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Services;
using Microsoft.AspNetCore.Components;

namespace Cyrena.Components.Tools
{
    public class DisplayServiceComponent : ComponentBase
    {
        [Inject] private DialogService _dialog { get; set; } = default!;
        [Inject] private ToastService _toasts { get; set; } = default!;
        [Inject] private NavigationManager _nav { get; set;  } = default!;
        [Inject] private IDisplayService _display { get; set; } = default!;

        protected override void OnInitialized()
        {
            ((DisplayService)_display).SetServices(_dialog, _toasts, _nav);
        }
    }
}
