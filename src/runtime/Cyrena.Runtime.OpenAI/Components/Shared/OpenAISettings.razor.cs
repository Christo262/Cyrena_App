using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Persistence.Contracts;
using Cyrena.Runtime.OpenAI.Models;
using Cyrena.Runtime.OpenAI.Options;
using Microsoft.AspNetCore.Components;

namespace Cyrena.Runtime.OpenAI.Components.Shared
{
    public partial class OpenAISettings
    {
        [Inject] private ISettingsService _settings { get; set; } = default!;
        [Inject] private IStore<OpenAIModel> _store { get; set;  } = default!;
        [Inject] private ToastService _toasts { get;set;  } = default!;
        [Inject] private DialogService _dialog { get; set; } = default!;

        private OpenAIOptions? _model;
        private IEnumerable<OpenAIModel> _models = Enumerable.Empty<OpenAIModel>();

        protected override void OnInitialized()
        {
            _model = _settings.Read<OpenAIOptions>(OpenAIOptions.Key) ?? new OpenAIOptions();
        }

        protected override async Task OnInitializedAsync()
        {
            _models = await _store.FindManyAsync(x => true);
        }

        private async Task Save()
        {
            if(_model == null) return;
            _settings.Save(OpenAIOptions.Key, _model);
            await _toasts.Success("OpenAI Settings", "OpenAI settings saved");
        }

        private async Task Add()
        {
            var model= new OpenAIModel();
            var result = await _dialog.ShowModal<OpenAIConnectionForm>(new ResultDialogOption()
            {
                Size = Size.Medium,
                Title = "Add Model",
                ComponentParameters = new()
                {
                    {"Model", model }
                },
                ButtonYesText = "Submit",
                ButtonNoText = "Cancel",
            });
            if(result == DialogResult.Yes)
            {
                await _store.AddAsync(model);
                _models = await _store.FindManyAsync(x => true);
                this.StateHasChanged();
            }
        }

        private async Task Edit(OpenAIModel model)
        {
            var result = await _dialog.ShowModal<OpenAIConnectionForm>(new ResultDialogOption()
            {
                Size = Size.Medium,
                Title = "Edit Model",
                ComponentParameters = new()
                {
                    {"Model", model }
                },
                ButtonYesText = "Submit",
                ButtonNoText = "Cancel",
            });
            if (result == DialogResult.Yes)
            {
                await _store.UpdateAsync(model);
                _models = await _store.FindManyAsync(x => true);
                this.StateHasChanged();
            }
        }

        private async Task Delete(OpenAIModel model)
        {
            var result = await _dialog.ShowModal("Delete Model", $"Are you sure you want to delete {model.DisplayName ?? model.ModelId}?", new ResultDialogOption()
            {
                Size = Size.Medium
            });
            if(result == DialogResult.Yes)
            {
                await _store.DeleteAsync(model);
                _models = await _store.FindManyAsync(x => true);
                this.StateHasChanged();
            }
        }
    }
}
