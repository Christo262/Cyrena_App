using Cyrena.Attributes;
using Cyrena.Dotnet.Contracts;
using Cyrena.Dotnet.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Cyrena.Dotnet.CSharp.Components.Shared
{
    public partial class SolutionViewer
    {
        [Inject] private ISnackbar _snackbar { get; set; } = default!;
        [KernelInject] private ISolutionController _sln { get; set; } = default!;
        [KernelInject] private IEnumerable<IDotnetProjectType> _projects { get; set; } = default!;

        private async Task SetActive(ProjectModel item)
        {
            try
            {
                await _sln.SetTargetProject(item);
                this.StateHasChanged();
            }
            catch (Exception ex)
            {
                _snackbar.Add(ex.Message, Severity.Error);
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
                _snackbar.Add(ex.Message, Severity.Error);
            }
        }
    }
}