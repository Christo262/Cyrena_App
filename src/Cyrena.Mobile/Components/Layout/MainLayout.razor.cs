using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Persistence;
using Cyrena.Persistence.Contracts;
using Cyrena.Extensions;
using Microsoft.AspNetCore.Components;
using Size = BootstrapBlazor.Components.Size;

namespace Cyrena.Mobile.Components.Layout
{
    public partial class MainLayout
    {
        [Inject] private IStore<ChatConfiguration> _store { get; set; } = default!;
        [Inject] private IKernelController _controller { get; set; } = default!;
        [Inject] private DialogService _dialog { get; set; } = default!;
        [Inject] private NavigationManager _nav { get; set; } = default!;

        private IEnumerable<ChatConfiguration>? _chats { get; set; }
        private IEnumerable<string?>? _groups { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            _controller.OnChatCreate((_) => this.InvokeAsync(Refresh));
            _controller.OnChatDelete((_) => this.InvokeAsync(Refresh));
            _controller.OnChatUpdate((_) => this.InvokeAsync(Refresh));
            await Refresh();
        }

        private async Task Refresh()
        {
            _chats = await _store.FindManyAsync(x => true, new OrderBy<ChatConfiguration>(x => x.LastModified, SortDirection.Descending));
            _groups = _chats.Select(x => x[ChatConfiguration.Group]).Distinct();
            this.StateHasChanged();
        }

        private async Task Delete(ChatConfiguration config)
        {
            var rf = await _dialog.ShowModal("Delete Chat", $"Are you sure you want to delete {config.Title ?? "this chat"}?", new ResultDialogOption()
            {
                Size = Size.Medium
            });
            if (rf == DialogResult.Yes)
            {
                if (_nav.Uri.EndsWith(config.Id))
                    _nav.NavigateTo("");
                await _controller.Delete(config);
                await Refresh();
            }
        }

        private bool _side { get; set; }
        private void ToggleSide() => _side = !_side;
    }
}
