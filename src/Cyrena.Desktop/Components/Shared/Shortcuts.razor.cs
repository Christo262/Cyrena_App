using Cyrena.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Desktop.Components.Shared
{
    public partial class Shortcuts
    {
        [Inject] private IServiceProvider _services { get; set; } = default!;

        private IEnumerable<IShortcut> _models = Enumerable.Empty<IShortcut>();

        protected override void OnInitialized()
        {
            _models = _services.GetServices<IShortcut>();
        }
    }
}
