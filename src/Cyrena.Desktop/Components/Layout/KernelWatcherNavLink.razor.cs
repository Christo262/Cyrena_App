using Cyrena.Contracts;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;

namespace Cyrena.Desktop.Components.Layout
{
    public partial class KernelWatcherNavLink : IDisposable
    {
        [Parameter]
        [EditorRequired]
        public ChatConfiguration ChatConfiguration { get; set; } = default!;

        [Inject] private IKernelController _controller { get; set; } = default!;
        [Inject] private NavigationManager _nav { get; set; } = default!;

        private IDisposable? _unload { get; set; }
        private IDisposable? _loaded { get; set; }
        private IDisposable? _update { get; set; }
        private IDisposable? _its_start { get; set; }
        private IDisposable? _its_end { get; set; }
        private bool _is_loaded { get; set; }
        private bool _is_its { get; set; }
        protected override void OnAfterRender(bool firstRender)
        {
            if (!firstRender)
                return;
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
                    if (_nav.Uri.EndsWith(ChatConfiguration.Id))
                        _nav.NavigateTo("");
                    else
                        this.InvokeAsync(StateHasChanged);
                }
            });

            _loaded = _controller.OnChatLoaded((cfg) =>
            {
                IsLoaded(cfg);
            });

            _update = _controller.OnChatUpdate((cfg) => 
            { 
                if(cfg.Id == ChatConfiguration.Id)
                {
                    ChatConfiguration = cfg;
                    this.InvokeAsync(StateHasChanged);
                }
            });
            if(_controller.KernelActive(ChatConfiguration.Id))
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

        public void Dispose()
        {
            _unload?.Dispose();
            _loaded?.Dispose();
            _update?.Dispose();
        }
    }
}
