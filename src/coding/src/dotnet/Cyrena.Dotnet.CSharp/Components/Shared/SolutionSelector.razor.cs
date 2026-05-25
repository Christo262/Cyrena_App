using Cyrena.Attributes;
using Cyrena.Contracts;
using Cyrena.Dotnet.Contracts;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Cyrena.Dotnet.CSharp.Components.Shared
{
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

        private async Task OnClick()
        {
            if (_its.Inferring)
                return;
            var parameters = new DialogParameters<SolutionViewer>
            {
                { nameof(SolutionViewer.Kernel), Kernel }
            };
            var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true };
            await _dialog.ShowAsync<SolutionViewer>("Projects", parameters, options);
        }
    }
}