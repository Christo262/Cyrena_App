using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using MudBlazor;
using static MudBlazor.CategoryTypes;

namespace Cyrena.Components.Layout
{
    public partial class KernelWatcherNavLink : IDisposable
    {
        [Parameter]
        [EditorRequired]
        public ChatConfiguration ChatConfiguration { get; set; } = default!;

        [Inject] private IKernelController _controller { get; set; } = default!;
        [Inject] private IServiceProvider _services { get; set; } = default!;
        [Inject] private IDialogService _dialog { get; set; } = default!;
        [Inject] private NavigationManager _nav { get; set; } = default!;
        [Inject] private IWindowLauncher _windows { get; set; } = default!;
        [Inject] private ComponentOptions _ui { get; set; } = default!;

        private IDisposable? _unload { get; set; }
        private IDisposable? _loaded { get; set; }
        private IDisposable? _update { get; set; }
        private IDisposable? _its_start { get; set; }
        private IDisposable? _its_end { get; set; }
        private bool _is_loaded { get; set; }
        private bool _is_its { get; set; }
        private bool _open { get; set; }

        protected override void OnInitialized()
        {
            _unload = _controller.OnChatUnload((cfg) =>
            {
                if (cfg.Id == ChatConfiguration.Id)
                {
                    _is_loaded = false;
                    _is_its = false;
                    _its_start?.Dispose();
                    _its_end?.Dispose();
                    _its_start = null;
                    _its_end = null;
                        this.InvokeAsync(StateHasChanged);
                }
            });

            _loaded = _controller.OnChatLoaded((cfg) =>
            {
                IsLoaded(cfg);
            });

            _update = _controller.OnChatUpdate((cfg) =>
            {
                if (cfg.Id == ChatConfiguration.Id)
                {
                    ChatConfiguration = cfg;
                    this.InvokeAsync(StateHasChanged);
                }
            });
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (!firstRender) return;
            if (_controller.KernelActive(ChatConfiguration.Id))
                IsLoaded(ChatConfiguration);
        }

        private void IsLoaded(ChatConfiguration cfg)
        {
            if (cfg.Id == ChatConfiguration.Id)
            {
                _is_loaded = true;
                var its = _controller.GetKernel(cfg.Id)?.GetRequiredService<IIterationService>();
                if (its != null)
                {
                    _its_start = its.OnIterationStart(e =>
                    {
                        _is_its = true;
                        this.InvokeAsync(StateHasChanged);
                    });
                    _its_end = its.OnIterationEnd(e =>
                    {
                        _is_its = false;
                        this.InvokeAsync(StateHasChanged);
                    });
                    _is_its = its.Inferring;
                }
                this.InvokeAsync(StateHasChanged);
            }
        }

        private async Task Delete()
        {
            _open = false;
            bool? result = await _dialog.ShowMessageBoxAsync(
                "Delete Chat",
                $"Are you sure you want to delete {ChatConfiguration.Title ?? "this chat"}?",
                yesText: "Delete", cancelText: "Cancel");
            if (result == true)
                await _controller.Delete(ChatConfiguration);
        }

        private async Task EditAsync()
        {
            var asst = _services.GetServices<IAssistantMode>().FirstOrDefault(x => x.Id == ChatConfiguration.AssistantModeId);
            if (asst == null)
            {
                await _dialog.ShowMessageBoxAsync("Error", "Unable to find configuration service for this chat.");
                return;
            }
            await asst.EditAsync(ChatConfiguration, _services);
        }

        private void Load()
        {
            _open = false;
            _nav.NavigateTo(_ui.MenuOptions.ConverseUrl.Replace("{Id}", ChatConfiguration.Id));
        }

        private void NewWindow()
        {
            if (!_ui.MenuOptions.AllowNewTab) return;
            var options = _services.GetRequiredService<ISettingsService>().Read<ApplicationOptions>(ApplicationOptions.Key) ?? new ApplicationOptions();
            _open = false;
            _windows.Show($"{_nav.BaseUri.TrimEnd('/')}/converse-blank/{ChatConfiguration.Id}", options.Width, options.Height);
        }

        public void Dispose()
        {
            _unload?.Dispose();
            _loaded?.Dispose();
            _update?.Dispose();
        }
    }
}
