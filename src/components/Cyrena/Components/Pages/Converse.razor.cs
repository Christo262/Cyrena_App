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

        [Inject] private ISnackbar _snackbar { get; set; } = null!;
        [Inject] private IKernelController _controller { get; set; } = null!;
        [Inject] private NavigationManager _nav { get; set; } = null!;
        [Inject] private ComponentOptions _ui { get; set; } = null!;

        private List<IDisposable> _disposables { get; set; } = [];
        private Kernel? _kernel { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            if (string.IsNullOrEmpty(Id) || !_render) return;
            _loading = true;
            await Task.Delay(500);
            _kernel = await _controller.LoadAsync(Id);
            _loading = false;
            this.StateHasChanged();
        }
        private bool _render { get; set; }
        private bool _loading { get; set; } = true;
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            if (string.IsNullOrEmpty(Id)) return;
            _kernel = await _controller.LoadAsync(Id);
            _disposables.Add(_controller.OnChatUnload(cfg =>
            {
                if(cfg.Id == Id)
                    _nav.NavigateTo("");
            }));
            _render = true;
            _loading = false;
            this.StateHasChanged();
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
                disposable.Dispose();
        }
    }
}
