using BootstrapBlazor.Components;
using Cyrena.Attributes;
using Cyrena.Dotnet.Contracts;
using Cyrena.Dotnet.Models;
using Microsoft.AspNetCore.Components;

namespace Cyrena.Dotnet.CSharp.Components.Shared
{
    public partial class SolutionViewer : IResultDialog
    {
        [Inject] private ToastService _toasts { get; set; } = default!;
        [KernelInject] private ISolutionController _sln { get; set; } = default!;
        [KernelInject] private IEnumerable<IDotnetProjectType> _projects { get; set; } = default!;

        public Task OnClose(DialogResult result)
        {
            return Task.CompletedTask;
        }

        private async Task SetActive(ProjectModel item)
        {
            try
            {
                await _sln.SetTargetProject(item);
                this.StateHasChanged();
            }catch (Exception ex)
            {
                await _toasts.Error(ex.Message);
            }
        }

        private async Task Override(ProjectModel item)
        {
            try
            {
                await _sln.OverrideProjectType(item.Id, item.ProjectTypeId);
                this.StateHasChanged();
            }
            catch(Exception ex)
            {
                await _toasts.Error(ex.Message);
            }
        }
    }
}
