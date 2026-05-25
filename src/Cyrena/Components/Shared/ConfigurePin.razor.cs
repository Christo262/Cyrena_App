using Cyrena.Contracts;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Cyrena.Components.Shared
{
    public partial class ConfigurePin
    {
        [Parameter] public PinViewModel Model { get; set; } = default!;
        [Inject] private IPinService _pin { get; set; } = default!;
        [Inject] private ISnackbar _toasts { get; set; } = default!;

        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; } = default!;
        private MudForm _form = default!;
        private async Task Submit()
        {
            await _form.ValidateAsync();
            if (_form.IsValid)
            {
                if(Model.OldPin != Model.ConfirmOldPin)
                {
                    _toasts.Add("Incorrect PIN", Severity.Error);
                    return;
                }
                if(Model.NewPin != Model.ConfirmNewPin)
                {
                    _toasts.Add("Confirmation Incorrect", Severity.Error);
                    return;
                }
                MudDialog.Close(DialogResult.Ok(Model));
            }

        }
    }
}
