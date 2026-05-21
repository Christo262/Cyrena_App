using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Desktop.Components.Shared;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Persistence;
using Cyrena.Persistence.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Photino.NET;

namespace Cyrena.Desktop.Components.Layout
{
    public partial class MainLayout
    {
        [Inject] private IStore<ChatConfiguration> _store { get; set; } = default!;
        [Inject] private IServiceProvider _services { get; set; } = default!;
        [Inject] private IKernelController _controller { get; set; } = default!;
        [Inject] private DialogService _dialog { get; set; } = default!;
        [Inject] private NavigationManager _nav { get;set;  } = default!;
        [Inject] private IViewStart _start { get; set; } = default!;
        [Inject] private PhotinoWindow _window { get; set; } = default!;

        private IEnumerable<ChatConfiguration>? _chats { get; set; }
        private IEnumerable<string?>? _groups { get; set; }
        private ViewStart _view_start = default!;

        private bool _loading { get; set; }

        protected override void OnInitialized()
        {
            _view_start = _start.GetViewStart();
            _nav.NavigateTo(_view_start.Href);
            base.OnInitialized();
        }

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
            _controller.OnChatLoadStart(cht =>
            {
                _loading = true;
                this.InvokeAsync(StateHasChanged);
            });
            _controller.OnChatLoaded(cht =>
            {
                _loading = false;
                this.InvokeAsync(StateHasChanged);
            });
            _controller.OnChatLoadError(ex =>
            {
                _loading = false;
                this.InvokeAsync(StateHasChanged);
            });
            await Refresh();

            if (Environment.OSVersion.Platform == PlatformID.Unix)
                await WebViewResize();
        }

        private async Task Refresh()
        {
            _chats = await _store.FindManyAsync(x => true, new OrderBy<ChatConfiguration>(x => x.LastModified, SortDirection.Descending));
            _groups = _chats.Select(x => x[ChatConfiguration.Group]).Distinct();
            this.StateHasChanged();
        }

        private async Task WebViewResize()
        {
            var size = _window.Size;
            _window.SetSize(size.Width +1, size.Height +1);
            await Task.Delay(50);
            _window.SetSize(size);
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
