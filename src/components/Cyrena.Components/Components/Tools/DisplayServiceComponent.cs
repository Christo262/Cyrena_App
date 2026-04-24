using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Components.Tools
{
    public class DisplayServiceComponent : KernelComponentBase
    {
        [Inject] private DialogService _dialog { get; set; } = default!;
        [Inject] private ToastService _toasts { get; set; } = default!;
        [Inject] private NavigationManager _nav { get; set;  } = default!;

        protected override void OnInitialized()
        {
            var dsp = Kernel.Services.GetRequiredService<IDisplayService>();
            ((DisplayService)dsp).SetServices(_dialog, _toasts, _nav);
        }
    }
}
