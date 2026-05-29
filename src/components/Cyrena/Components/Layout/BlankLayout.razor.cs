using Cyrena.Contracts;
using Cyrena.Options;
using Microsoft.AspNetCore.Components;

namespace Cyrena.Components.Layout
{
    public partial class BlankLayout
    {
        [Inject] private ISettingsService _settings { get; set; } = default!;
        private bool _isDarkMode = true;
        protected override void OnInitialized()
        {
            var options = _settings.Read<ApplicationOptions>(ApplicationOptions.Key) ?? new ApplicationOptions();
            _isDarkMode = options.DarkMode;
            base.OnInitialized();
        }
    }
}
