using Cyrena.Contracts;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;

namespace Cyrena.Components.Shared
{
    public partial class AuthorizedState : IDisposable
    {
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Inject] private IPinService _pin { get; set; } = default!;

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            if (!_rendered) return;
            if (!_pin.IsAuthorized())
                await _pin.AuthorizeAsync();
        }

        protected override void OnInitialized()
        {
            ApplicationTheme.WatchTheme(() =>
            {
                this.InvokeAsync(StateHasChanged);
            });
            _pin.AuthorizationChanged += _pin_AuthorizationChanged;
            base.OnInitialized();
        }

        private void _pin_AuthorizationChanged(object? sender, bool e)
        {
            this.InvokeAsync(async () =>
            {
                if(!_pin.IsAuthorized())
                    await _pin.AuthorizeAsync();
                StateHasChanged();
            });
        }

        private bool _rendered { get; set; }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            if (!_pin.IsAuthorized())
                await _pin.AuthorizeAsync();
            _rendered = true;
        }

        public void Dispose()
        {
            _pin.AuthorizationChanged -= _pin_AuthorizationChanged;
        }
    }
}
