using Cyrena.Contracts;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using System.ComponentModel.DataAnnotations;

namespace Cyrena.Components.Shared
{
    public partial class EditDefaultAssistant
    {
        [Parameter] public ChatConfiguration Model { get; set; } = null!;
        private MudForm _form = null!;

        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; } = null!;

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
