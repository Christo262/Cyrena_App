using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Options;
using Cyrena.Persistence.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Extensions;

namespace Cyrena.Components.Layout
{
    public partial class MainLayout
    {
        [Inject] private IStore<ChatConfiguration> _store { get; set; } = default!;
        [Inject] private IKernelController _controller { get; set; } = default!;
        [Inject] private NavigationManager _nav { get;set;  } = default!;
        [Inject] private IViewStart _start { get; set; } = default!;
        [Inject] private ISettingsService _settings { get; set; } = default!;

        private IEnumerable<ChatConfiguration>? _chats { get; set; }
        private IEnumerable<string?>? _groups { get; set; }
        private ViewStart _view_start = default!;

        private bool _loading { get; set; }
        private bool _drawerOpen = true;

        protected override void OnInitialized()
        {
            var options = _settings.Read<ApplicationOptions>(ApplicationOptions.Key) ?? new ApplicationOptions();
            if(ApplicationTheme.DarkMode != options.DarkMode)
                ApplicationTheme.DarkMode = options.DarkMode;
            _view_start = _start.GetViewStart();
            base.OnInitialized();
        }

        private void ChangeTheme()
        {
            ApplicationTheme.DarkMode = !ApplicationTheme.DarkMode;
            var options = _settings.Read<ApplicationOptions>(ApplicationOptions.Key) ?? new ApplicationOptions();
            options.DarkMode = ApplicationTheme.DarkMode;
            _settings.Save(ApplicationOptions.Key, options);
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
        }

        private void DrawerToggle()
        {
            _drawerOpen = !_drawerOpen;
        }

        private async Task Refresh()
        {
            _chats = await _store.FindManyAsync(x => true);
            _groups = _chats.Select(x => x[ChatConfiguration.Group]).Distinct();
            this.StateHasChanged();
        }
    }
}
