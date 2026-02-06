using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Cyrena.Runtime.Ollama.Models;

namespace Cyrena.Runtime.Ollama.Components.Shared
{
    public partial class OllamaConnectionForm : IResultDialog
    {
        [Parameter] 
        public OllamaConnectionInfo Model { get; set; } = default!;

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
