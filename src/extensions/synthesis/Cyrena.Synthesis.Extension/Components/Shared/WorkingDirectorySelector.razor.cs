using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.SemanticKernel;

namespace Cyrena.Synthesis.Components.Shared
{
    public partial class WorkingDirectorySelector : IResultDialog
    {
        [Parameter] public string? Current { get; set; }
        [Inject] private IFileDialog _files { get; set; } = default!;

        private async Task Select()
        {
            var result = await _files.OpenAsync("Select Folder", null);
            if(!string.IsNullOrEmpty(result)) 
                Current = result;
        }

        Task IResultDialog.OnClose(DialogResult result)
        {
            return Task.CompletedTask;
        }

        async Task<bool> IResultDialog.OnClosing(DialogResult result)
        {
            if (result != DialogResult.Yes) return true;
            return !string.IsNullOrEmpty(Current);
        }
    }
}
