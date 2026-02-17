using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Cyrena.PlatformIO.Components.Shared
{
    public partial class Configure : IResultDialog
    {
        [Parameter] public Project Model { get; set; } = default!;
        [Inject] private IFileDialog _win { get; set; } = default!;

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
                var files = await _win.OpenAsync("Choose ini file", ("ini", [".ini"]));
                if (files != null)
                {
                    var info = new FileInfo(files);
                    Model.RootDirectory = info.DirectoryName ?? string.Empty;
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
