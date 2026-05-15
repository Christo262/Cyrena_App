using BootstrapBlazor.Components;
using Cyrena.Attributes;
using Cyrena.Contracts;
using Cyrena.Dotnet.Contracts;
using Microsoft.AspNetCore.Components;

namespace Cyrena.Dotnet.CSharp.Components.Shared
{
    public partial class SolutionSelector
    {
        [Inject] private DialogService _dialog { get; set; } = default!;
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

        private async Task OnClick()
        {
            if (_its.Inferring)
                return;
            var result = await _dialog.ShowModal<SolutionViewer>(new ResultDialogOption()
            {
                Size = Size.Medium,
                Title = "Projects",
                ShowNoButton = false,
                ButtonYesText = "Done",
                ComponentParameters = new()
                {
                    {nameof(SolutionViewer.Kernel), Kernel }
                }
            });
        }
    }
}
