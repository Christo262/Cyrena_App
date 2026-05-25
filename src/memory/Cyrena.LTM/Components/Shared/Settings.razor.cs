using Cyrena.Contracts;
using Cyrena.LTM.Contracts;
using Cyrena.LTM.Options;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Cyrena.LTM.Components.Shared
{
    public partial class Settings
    {
        [Inject] private ISettingsService _settings { get; set; } = default!;
        [Inject] private IMemoryService _ltm { get; set; } = default!;
        [Inject] private IDialogService _dialog { get; set; } = default!;
        [Inject] private ISnackbar _snackbar { get; set; } = default!;

        private MemoryContextOptions _model = new();

        protected override void OnInitialized()
        {
            _model = _settings.Read<MemoryContextOptions>(MemoryContextOptions.Key) ?? new MemoryContextOptions();
            _relevanceThreshold = (int)(_model.MinRelevanceThreshold * 10);
        }

        private void SaveChanges()
        {
            _settings.Save(MemoryContextOptions.Key, _model);
        }

        private int _relevanceThreshold { get; set; }

        private void SaveRelevance()
        {
            _model.MinRelevanceThreshold = (double)_relevanceThreshold / 10;
            _settings.Save(MemoryContextOptions.Key, _model);
        }

        private async Task ClearMemory()
        {
            var parameters = new DialogParameters<MudMessageBox>
            {
                { x => x.Title, "Clear Memory" },
                { x => x.Message, "Are you sure you want to clear all long-term memories?" },
                { x => x.YesText, "Yes" },
                { x => x.CancelText, "No" }
            };
            var dialog = await _dialog.ShowAsync<MudMessageBox>("", parameters);
            var result = await dialog.Result;

            if (result is { Canceled: false })
            {
                await _ltm.ClearMemoryAsync();
                _snackbar.Add("Long-Term memory successfully cleared", Severity.Success);
            }
        }
    }
}