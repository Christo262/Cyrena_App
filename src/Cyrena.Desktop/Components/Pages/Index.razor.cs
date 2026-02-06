using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Persistence;
using Cyrena.Persistence.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Desktop.Components.Pages
{
    public partial class Index
    {
        [Inject] private IStore<Project> _store { get; set; } = default!;
        [Inject] private IServiceProvider _services { get; set; } = default!;
        [Inject] private ToastService _toasts { get; set; } = default!;

        private IEnumerable<Project>? _projects { get; set; }

        public override async Task OnFirstRenderAsync()
        {
            _projects = await _store.FindManyAsync(x => true, new OrderBy<Project>(x => x.LastModified, SortDirection.Descending));
            this.StateHasChanged();
        }

        private async Task Refresh()
        {
            _projects = await _store.FindManyAsync(x => true, new OrderBy<Project>(x => x.LastModified, SortDirection.Descending));
            this.StateHasChanged();
        }

        private async Task Edit(Project project)
        {
            var cfgs = _services.GetServices<IProjectConfigurator>();
            var cfg = cfgs.FirstOrDefault(x => x.ProjectType == project.Type);
            if(cfg == null)
            {
                await _toasts.Error("Configurator Missing", "Configuration service for this project type not found.");
                return;
            }
            await cfg.EditAsync(project);
        }
    }
}
