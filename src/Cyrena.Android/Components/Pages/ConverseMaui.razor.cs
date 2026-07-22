using Cyrena.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.SemanticKernel;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Android.Components.Pages
{
    public partial class ConverseMaui
    {
        [Parameter] public string? Id { get; set; }
        [Inject] private ISnackbar _snackbar { get; set; } = default!;
        [Inject] private IKernelController _controller { get; set; } = default!;
        [Inject] private NavigationManager _nav { get; set; } = default!;

        private Kernel? _kernel { get; set; }
        private bool _loading = true;
        private IDisposable? _watcher { get; set; }
        private bool _rendered { get; set; }
        protected override async Task OnParametersSetAsync()
        {
            if (!_rendered) return;
            _loading = true;
            await Task.Delay(20);
            try
            {
                _watcher?.Dispose();
                if (string.IsNullOrEmpty(Id))
                    throw new NullReferenceException("No id provided");
                _kernel = await _controller.LoadAsync(Id);
                _watcher = _controller.OnChatUnload(cfg =>
                {
                    if (cfg.Id == Id)
                    {
                        _kernel = null;
                        _nav.NavigateTo("");
                    }
                });
            }
            catch (Exception ex)
            {
                _snackbar.Add(ex.Message, Severity.Error);
            }
            _loading = false;
            this.StateHasChanged();
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            try
            {
                if (string.IsNullOrEmpty(Id))
                    throw new NullReferenceException("No id provided");
                _kernel = await _controller.LoadAsync(Id);
                _watcher = _controller.OnChatUnload(cfg =>
                {
                    if (cfg.Id == Id)
                    {
                        _kernel = null;
                        _nav.NavigateTo("");
                    }
                });
            }
            catch (Exception ex)
            {
                _snackbar.Add(ex.Message, Severity.Error);
            }
            _loading = false;
            _rendered = true;
            this.StateHasChanged();
        }

        public void Dispose()
        {
            _watcher?.Dispose();
        }
    }
}
