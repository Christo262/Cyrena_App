using Microsoft.AspNetCore.Components;
using MudBlazor;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Persistence;
using Cyrena.Persistence.Contracts;
using Cyrena.Runtime.OpenAI.Models;
using Cyrena.Runtime.OpenAI.Options;

namespace Cyrena.Runtime.OpenAI.Components.Shared
{
    public partial class OpenAISettings
    {
        [Inject] private ISettingsService _settings { get; set; } = default!;
        [Inject] private IStore<OpenAIModel> _store { get; set; } = default!;
        [Inject] private ISnackbar _snackbar { get; set; } = default!;
        [Inject] private IDialogService _dialog { get; set; } = default!;

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

        private void Save()
        {
            if (_model == null) return;
            _settings.Save(OpenAIOptions.Key, _model);
            _snackbar.Add("OpenAI settings saved", Severity.Success);
        }

        private async Task Add()
        {
            var model = new OpenAIModel();
            var parameters = new DialogParameters<OpenAIConnectionForm>
            {
                { x => x.Model, model }
            };
            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
            var dialog = await _dialog.ShowAsync<OpenAIConnectionForm>("Add Model", parameters, options);
            var result = await dialog.Result;
            if (result is { Canceled: false })
            {
                await _store.AddAsync(model);
                _models = await _store.FindManyAsync(x => true);
                this.StateHasChanged();
            }
        }

        private async Task Edit(OpenAIModel model)
        {
            var parameters = new DialogParameters<OpenAIConnectionForm>
            {
                { x => x.Model, model }
            };
            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
            var dialog = await _dialog.ShowAsync<OpenAIConnectionForm>("Edit Model", parameters, options);
            var result = await dialog.Result;
            if (result is { Canceled: false })
            {
                await _store.UpdateAsync(model);
                _models = await _store.FindManyAsync(x => true);
                this.StateHasChanged();
            }
        }

        private async Task Delete(OpenAIModel model)
        {
            var parameters = new DialogParameters<MudMessageBox>
            {
                { x => x.Title, "Delete Model" },
                { x => x.Message, $"Are you sure you want to delete {model.DisplayName ?? model.ModelId}?" },
                { x => x.YesText, "Delete" },
                { x => x.CancelText, "Cancel" }
            };
            var dialog = await _dialog.ShowAsync<MudMessageBox>("", parameters);
            var result = await dialog.Result;
            if (result is { Canceled: false })
            {
                await _store.DeleteAsync(model);
                _models = await _store.FindManyAsync(x => true);
                this.StateHasChanged();
            }
        }
    }
}