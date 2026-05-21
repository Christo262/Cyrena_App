using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.SemanticKernel;

namespace Cyrena.Components.Pages
{
    public partial class Converse
    {
        [Parameter] public string? Id { get; set; }
        [CascadingParameter]
        public TabItem? Item { get; set; }
        [CascadingParameter]
        public Tab? Parent { get; set; }
        [Inject] private ToastService _toasts { get; set; } = default!;
        [Inject] private IKernelController _controller { get; set; } = default!;
        [Inject] private NavigationManager _nav { get; set; } = default!;

        private Kernel? _kernel { get; set; }
        private IDisposable? _watcher { get; set; }
        private IDisposable? _updater { get; set; }

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
                if (Item != null)
                {
                    var config = _kernel.GetRequiredService<IChatConfigurationService>();
                    Item.SetHeader(config.Config.Title ?? "New Chat", config.Config[ChatConfiguration.Icon]);
                    _watcher = _controller.OnChatUnload(async (cfg) =>
                    {
                        if (cfg.Id == config.Config.Id)
                        {
                            _nav.NavigateTo("");
                            await Task.Delay(50);
                            if(Parent != null && Item != null)
                                await Parent.RemoveTab(Item);
                        }
                    });
                    _updater = _controller.OnChatUpdate((cfg) =>
                    {
                        if (cfg.Id == config.Config.Id && Item != null)
                            Item.SetHeader(config.Config.Title ?? "New Chat", config.Config[ChatConfiguration.Icon]);
                    });
                }
                this.StateHasChanged();
            }
            catch (Exception ex)
            {
                await _toasts.Error("Error", ex.Message);
                _nav.NavigateTo("");
                await Task.Delay(50);
                if (Parent != null && Item != null)
                    await Parent.RemoveTab(Item);
            }
        }

        public void Dispose()
        {
            _watcher?.Dispose();
            _updater?.Dispose();
        }
    }
}
