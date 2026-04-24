using Cyrena.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.HUD.Components.Shared
{
    public partial class Shortcuts
    {
        [Inject] private IServiceProvider _services { get; set; } = default!;

        private IEnumerable<IShortcut> _models = Enumerable.Empty<IShortcut>();
        private IEnumerable<string> _categories = Enumerable.Empty<string>();

        protected override void OnInitialized()
        {
            _models = _services.GetServices<IShortcut>();
            _categories = _models.Select(x => x.Category).Distinct();
        }
    }
}
