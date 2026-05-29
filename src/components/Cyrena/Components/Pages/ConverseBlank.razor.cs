using Cyrena.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.SemanticKernel;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Components.Pages
{
    public partial class ConverseBlank : IDisposable
    {
        [Parameter] public string? Id { get; set; }
        [Inject] private ISnackbar _snackbar { get; set; } = default!;
        [Inject] private IKernelController _controller { get; set; } = default!;
        [Inject] private NavigationManager _nav { get; set; } = default!;

        private Kernel? _kernel { get; set; }
        private bool _loading = true;
        private IDisposable? _watcher { get; set; }

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
                        this.InvokeAsync(() => StateHasChanged());
                    }
                });
            }catch (Exception ex)
            {
                _snackbar.Add(ex.Message, Severity.Error);
            }
            _loading = false;
            this.StateHasChanged();
        }

        public void Dispose()
        {
            _watcher?.Dispose();
        }
    }
}
