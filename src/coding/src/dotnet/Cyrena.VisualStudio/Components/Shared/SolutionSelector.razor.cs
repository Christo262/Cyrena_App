using Cyrena.Attributes;
using Cyrena.Contracts;
using Cyrena.Dotnet.Contracts;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Cyrena.VisualStudio.Components.Shared;

public partial class SolutionSelector
{
    [Inject] private IDialogService _dialog { get; set; } = default!;
    [KernelInject] private ISolutionController _sln { get; set; } = default!;
    [KernelInject] private IIterationService _its { get; set; } = default!;
    protected override void OnInitialized()
    {
        _its.OnIterationStart(e => this.InvokeAsync(StateHasChanged));
        _its.OnIterationEnd(e => this.InvokeAsync(StateHasChanged));
        _sln.OnProjectChange(p =>
        {
            this.InvokeAsync(StateHasChanged);
        });
    }

    private void OnSlnChange(string? id)
    {
        var proj = _sln.GetValidProjects().FirstOrDefault(p => p.Id == id);
        if (proj != null)
        {
            _sln.SetTargetProject(proj);
        }
    }
}