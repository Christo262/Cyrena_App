using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.SemanticKernel;

namespace Cyrena.Mobile.Components.Pages
{
    public partial class Converse
    {
        [Parameter] public string? Id { get; set; }
        [Inject] private ToastService _toasts { get; set; } = default!;
        [Inject] private IKernelController _controller { get; set; } = default!;
        [Inject] private NavigationManager _nav { get; set; } = default!;

        private Kernel? _kernel { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            try
            {
                if (string.IsNullOrEmpty(Id))
                    throw new Exception("No ID provided");
                _kernel = null;
                this.StateHasChanged();
                await Task.Delay(1);
                _kernel = await _controller.LoadAsync(Id);
                if (_kernel == null)
                    throw new Exception($"Kernel not loaded");
                this.StateHasChanged();
            }
            catch (Exception ex)
            {
                await _toasts.Error("Error", ex.Message);
                _nav.NavigateTo("");
            }
        }
    }
}
