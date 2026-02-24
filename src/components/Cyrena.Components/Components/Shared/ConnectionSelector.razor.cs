using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Cyrena.Contracts;
using Cyrena.Models;

namespace Cyrena.Components.Shared
{
    public partial class ConnectionSelector
    {
        [Inject] private IServiceProvider _services { get; set; } = default!;
        [Parameter] public string? Value { get; set; }
        [Parameter] public EventCallback<string> ValueChanged { get; set; }

        private IEnumerable<IConnectionProvider> _providers = default!;
        private List<ConnectionInfo> _models { get; set; } = new();

        protected override void OnInitialized()
        {
            _providers = _services.GetServices<IConnectionProvider>();
        }

        private void OnValueChanged()
        {
            ValueChanged.InvokeAsync(Value);
        }

        private async Task Populate()
        {
            _models.Clear();
            foreach (var item in _providers)
            {
                var infos = await item.ListConnectionsAsync();
                _models.AddRange(infos);
            }
        }
    }
}
