using Cyrena.Contracts;
using Cyrena.Coding.Contracts;
using Cyrena.Coding.Models;
using Microsoft.AspNetCore.Components;

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

        protected override void OnParametersSet()
        {
            _models = _versions.GetBackups();
        }
    }
}
