using Cyrena.Extensa.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace Cyrena.Extensa.Components.Shared
{
    public partial class ServerForm
    {
        [Parameter]
        public PluginServer Model { get; set; } = default!;

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
