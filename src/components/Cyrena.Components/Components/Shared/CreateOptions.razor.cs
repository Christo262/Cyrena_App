using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Cyrena.Contracts;

namespace Cyrena.Components.Shared
{
    public partial class CreateOptions
    {
        [Inject] private IServiceProvider _services { get; set; } = default!;
        [Parameter] public EventCallback OnAfterCreate { get; set; }

        private IEnumerable<IProjectConfigurator> _models { get; set;} = Enumerable.Empty<IProjectConfigurator>();

        protected override void OnAfterRender(bool firstRender)
        {
            if (!firstRender) return;
            _models = _services.GetServices<IProjectConfigurator>();
            this.StateHasChanged();
        }

        private async Task CreateAsync(IProjectConfigurator projectConfigurator)
        {
            bool e = await projectConfigurator.CreateNewAsync();
            if(e)
                await OnAfterCreate.InvokeAsync();
        }
    }
}
