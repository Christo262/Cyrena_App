using BootstrapBlazor.Components;
using Cyrena.Extensa.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Cyrena.Extensa.Components.Shared
{
    public partial class ServerForm : IResultDialog
    {
        [Parameter]
        public PluginServer Model { get; set; } = default!;

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
    }
}
