using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.SemanticKernel;
using MudBlazor;

namespace Cyrena.Components.Pages
{
    public partial class Converse : IDisposable
    {
        [Parameter] public string? Id { get; set; }

        [Inject] private ISnackbar _snackbar { get; set; } = default!;
        [Inject] private IKernelController _controller { get; set; } = default!;
        [Inject] private NavigationManager _nav { get; set; } = default!;
        [Inject] private ComponentOptions _ui { get; set; } = default!;

        private List<Kernel> _active { get; set; } = [];

        private List<IDisposable> _disposables = [];
        private MudDynamicTabs _dynamicTabs = null!;
        private int _userIndex;

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            if(!string.IsNullOrEmpty(Id))
                try
                {
                    var ext = await _controller.LoadAsync(Id);
                    if (_active.Count == 0) _active = _controller.ActiveKernels.ToList();
                    if(ext != null)
                    {
                        if (!_active.Contains(ext))
                            _active.Add(ext);
                        _userIndex = _active.IndexOf(ext);
                    }
                }
                catch (Exception ex)
                {
                    _snackbar.Add(ex.Message, Severity.Error);
                }
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (!firstRender) return;
            _disposables.Add(_controller.OnChatLoaded(OnChatLoaded));
            _disposables.Add(_controller.OnChatUnload(OnChatUnloaded));
            this.StateHasChanged();
        }

        private void OnChatLoaded(ChatConfiguration chat)
        {
            this.InvokeAsync(() =>
            {
                var kernel = _controller.GetKernel(chat.Id);
                if (kernel != null)
                {
                    _active.Add(kernel);
                    var idx = _active.IndexOf(kernel);
                    _userIndex = idx;
                }
                StateHasChanged();
            });
        }

        private void OnChatUnloaded(ChatConfiguration chat)
        {
            this.InvokeAsync(() =>
            {
                var item = _active.FirstOrDefault(x => x.GetId() == chat.Id);
                if (item is not null)
                    _active.Remove(item);
                if (_active.Count == 0)
                    _nav.NavigateTo("");
                StateHasChanged();
            });
        }

        private void OnTabChange(int index)
        {
            _userIndex = index;
            var active = _dynamicTabs.ActivePanel;
            if (active != null && active.ID is Kernel kernel)
                _nav.NavigateTo(_ui.MenuOptions.ConverseUrl.Replace("{Id}", kernel.GetId()));
        }

        public void Dispose()
        {
            foreach(var dsp in _disposables) dsp.Dispose();
            _disposables.Clear();
        }

        private async Task OnCloseTab(MudTabPanel panel)
        {
            if(panel.ID is Kernel instance)
            {
                _controller.Unload(instance.GetConfiguration());
            }
        }
    }
}
