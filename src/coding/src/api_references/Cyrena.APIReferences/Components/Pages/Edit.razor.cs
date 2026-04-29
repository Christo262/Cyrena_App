using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.APIReferences.Models;
using Cyrena.Extensions;
using Cyrena.Persistence.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.APIReferences.Components.Pages
{
    public partial class Edit : IDisposable
    {
        [Inject] private IKernelController _kernels { get; set; } = default!;
        [Inject] private NavigationManager _nav { get; set; } = default!;
        [Inject] private ToastService _toasts { get; set; } = default!;
        [Parameter] public string? RefId { get; set; }
        [Parameter] public string? KernelId { get; set; }
        [CascadingParameter]
        public TabItem? Item { get; set; }
        [CascadingParameter]
        public Tab? Parent { get; set; }
        private IDisposable _unload = default!;

        private Kernel _kernel = default!;
        private IStore<ApiReference> _store = default!;
        private ApiReference? _model { get; set; }
        private string? _keywords { get; set; }

        protected override void OnInitialized()
        {
            _unload = _kernels.OnChatUnload(cfg =>
            {
                if (cfg.Id == KernelId)
                {
                    if (Item != null && Parent != null)
                        Parent.RemoveTab(Item);
                }
            });
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            try
            {
                if (string.IsNullOrEmpty(KernelId))
                    throw new NullReferenceException("Kernel Id not provided");

                var kernel = _kernels.GetKernel(KernelId);
                if (kernel == null)
                    throw new NullReferenceException("Unable to find instance of Kernel");
                _kernel = kernel;
                _store = _kernel.Services.GetRequiredService<IStore<ApiReference>>();
                if(string.IsNullOrEmpty(RefId))
                    _model = new ApiReference() { Id = Guid.NewGuid().ToString() };
                else
                    _model = await _store.FindAsync(x => x.Id == RefId);
                if (_model == null)
                    throw new NullReferenceException("Unable to find API Reference");
                _keywords = string.Join(", ", _model.Keywords);
                this.StateHasChanged();
            }
            catch (Exception ex)
            {
                await _toasts.Error("Error", ex.Message);
                if (Parent != null && Item != null)
                    await Parent.RemoveTab(Item);
                _nav.NavigateTo("");
            }
        }

        private async Task SaveAsync()
        {
            if (_model == null) return;
            if(_keywords != null)
                _model.Keywords = _keywords.Split(",").Select(x => x.Trim()).ToArray();
            await _store.SaveAsync(_model);
            if (Parent != null && Item != null)
                await Parent.RemoveTab(Item);
        }

        private async Task Cancel()
        {
            if (Parent != null && Item != null)
                await Parent.RemoveTab(Item);
        }

        public void Dispose()
        {
            _unload.Dispose();
        }
    }
}
