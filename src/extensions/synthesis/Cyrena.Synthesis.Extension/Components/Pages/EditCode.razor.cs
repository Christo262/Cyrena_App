using BootstrapBlazor.Components;
using Cyrena.Synthesis.Contracts;
using Cyrena.Synthesis.Models;
using Cyrena.Synthesis.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Synthesis.Components.Pages
{
    public partial class EditCode
    {
        [Parameter] public string? Id { get; set; }

        [CascadingParameter] public TabItem? Tab { get; set; }
        [CascadingParameter] public Tab? Parent { get; set; }

        [Inject] private ICapabilityStore _store { get; set; } = default!;
        [Inject] private NavigationManager _nav { get; set; } = default!;
        [Inject] private ToastService _toasts { get; set; } = default!;

        private DynamicCapability? _model { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            if (string.IsNullOrEmpty(Id))
            {
                if(Parent is not null && Tab is not null)
                    await Parent.RemoveTab(Tab);
                _nav.NavigateTo("");
                return;
            }
            _model = await _store.GetByIdAsync(Id);
            if(_model == null)
            {
                if (Parent is not null && Tab is not null)
                    await Parent.RemoveTab(Tab);
                await _toasts.Error($"Unable to find capability with id {Id}");
                _nav.NavigateTo("");
                return;
            }
            this.StateHasChanged();
        }

        private async Task Save()
        {
            if (_model == null) return;
            await _store.SaveAsync(_model);
            await _toasts.Information("Saved", "Code changes saved");
        }

        private ScriptValidationResult? _validation;
        private async Task Validate()
        {
            if (_model == null) return;
            var validator = new ScriptValidator();
            _validation = await validator.ValidateAsync(_model.Code);
        }
    }
}
