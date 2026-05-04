using Cyrena.Contracts;
using Cyrena.Coding.Contracts;
using Cyrena.Coding.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Cyrena.Coding.Components.Shared
{
    public partial class VersionControlViewer
    {
        private IVersionControl _versions = default!;
        private IChatConfigurationService _chat = default!;
        private IEnumerable<DevelopFileContent> _models = Enumerable.Empty<DevelopFileContent>();
        [Inject] private NavigationManager _nav { get; set; } = default!;
        protected override void OnInitialized()
        {
            _versions = Kernel.GetRequiredService<IVersionControl>();
            _chat = Kernel.GetRequiredService<IChatConfigurationService>();
        }

        private bool _show { get; set; }
        private void Toggle()
        {
            _show = !_show;
            if (_show)
                _models = _versions.GetBackups();
        }
    }
}
