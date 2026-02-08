using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Cyrena.Models;
using Cyrena.Contracts;
using Cyrena.Net.Models;

namespace Cyrena.Blazor.Components.Shared
{
    public partial class BlazorConfig : IResultDialog
    {
        [Parameter] public DotnetProject Model { get; set; } = default!;
        [Inject] private ICurrentWindow _win { get; set;  } = default!;

        private EditContext _context = default!;

        protected override void OnInitialized()
        {
            _context = new EditContext(Model);
        }

        Task IResultDialog.OnClose(DialogResult result)
        {
            return Task.CompletedTask;
        }

        async Task<bool> IResultDialog.OnClosing(DialogResult result)
        {
            if (result != DialogResult.Yes) return true;
            var valid = _context.Validate();
            return valid;
        }

        private async Task PickProject()
        {
            try
            {
                var files = await _win.ShowFileSelect("Choose csproj", "csproj", [".csproj"]);
                if (files.Length > 0)
                {
                    var info = new FileInfo(files[0]);
                    Model.RootDirectory = info.DirectoryName ?? string.Empty;
                }
            }catch (Exception ex)
            {

            }
        }
    }
}
