using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Desktop.Components.Shared;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Persistence;
using Cyrena.Persistence.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Desktop.Components.Layout
{
    public partial class MainLayout
    {
        [Inject] private IStore<ChatConfiguration> _store { get; set; } = default!;
        [Inject] private IServiceProvider _services { get; set; } = default!;
        [Inject] private IKernelController _controller { get; set; } = default!;
        [Inject] private DialogService _dialog { get; set; } = default!;
        [Inject] private NavigationManager _nav { get;set;  } = default!;

        private IEnumerable<ChatConfiguration>? _chats { get; set; }
        private IEnumerable<string?>? _groups { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            _controller.OnChatCreate((_) => this.InvokeAsync(Refresh));
            _controller.OnChatDelete((config) =>
            {         
                this.InvokeAsync(async () =>
                {
                    if (_nav.Uri.EndsWith(config.Id))
                        _nav.NavigateTo("");
                    await Refresh();
                });
            });
            _controller.OnChatUpdate((_) => this.InvokeAsync(Refresh));
            _controller.OnChatUnload((config) =>
            {
                this.InvokeAsync(async () =>
                {
                    if (_nav.Uri.EndsWith(config.Id))
                        _nav.NavigateTo("");
                    await Refresh();
                });
            });
            await Refresh();
        }

        private async Task Refresh()
        {
            _chats = await _store.FindManyAsync(x => true, new OrderBy<ChatConfiguration>(x => x.LastModified, SortDirection.Descending));
            _groups = _chats.Select(x => x[ChatConfiguration.Group]).Distinct();
            this.StateHasChanged();
        }

        private Task Unload(ChatConfiguration config)
        {
            _controller.Unload(config);
            return Task.CompletedTask;
        }

        private async Task Delete(ChatConfiguration config)
        {
            var rf = await _dialog.ShowModal("Delete Chat", $"Are you sure you want to delete {config.Title ?? "this chat"}?", new ResultDialogOption()
            {
                Size = Size.Medium
            });
            if(rf == DialogResult.Yes)
                await _controller.Delete(config);
        }

        private async Task EditAsync(ChatConfiguration config)
        {
            var asst = _services.GetServices<IAssistantMode>().FirstOrDefault(x => x.Id == config.AssistantModeId);
            if (asst == null)
            {
                await _dialog.ShowModal("Error", "Unable to find configuration service for this chat.", new ResultDialogOption()
                {
                    Size = Size.Medium,
                    ShowCloseButton = true,
                    ShowNoButton = false,
                    ButtonYesText = "Okay"
                });
                return;
            }
            await asst.EditAsync(config, _services);
        }

        private bool IsActive(ChatConfiguration config) => _controller.KernelActive(config.Id);

        private bool _side { get; set; }
        private void ToggleSide() => _side = !_side;
    }
}
