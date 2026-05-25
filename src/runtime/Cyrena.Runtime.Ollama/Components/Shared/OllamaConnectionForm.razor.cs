using Microsoft.AspNetCore.Components;
using MudBlazor;
using Cyrena.Runtime.Ollama.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace Cyrena.Runtime.Ollama.Components.Shared
{
    public partial class OllamaConnectionForm
    {
        [Parameter] 
        public OllamaConnectionInfo Model { get; set; } = default!;

        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; } = default!;

        private MudForm _form = default!;

        private async Task Submit()
        {       
            await _form.ValidateAsync();
            if (_form.IsValid)
                MudDialog.Close(DialogResult.Ok(Model));
        }

        private void Cancel()
        {
            MudDialog.Close(DialogResult.Cancel());
        }
    }
}