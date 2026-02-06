using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Cyrena.Contracts;
using Cyrena.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Components.Shared
{
    public partial class ConnectionSelector
    {
        [Inject] private IServiceProvider _services { get; set; } = default!;
        [Parameter] public string? Value { get; set; }
        [Parameter] public EventCallback<string> ValueChanged { get; set; }

        private List<ConnectionInfo> _models { get; set; } = new();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            var providers = _services.GetServices<IConnectionProvider>();
            foreach(var item in providers)
            {
                var infos = await item.ListConnectionsAsync();
                _models.AddRange(infos);
            }
            this.StateHasChanged();
        }

        private void OnValueChanged()
        {
            ValueChanged.InvokeAsync(Value);
        }
    }
}
