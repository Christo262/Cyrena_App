using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.LTM.Contracts;
using Cyrena.LTM.Options;
using Microsoft.AspNetCore.Components;

namespace Cyrena.LTM.Components.Shared
{
    public partial class Settings
    {
        [Inject] private ISettingsService _settings { get; set; } = default!;
        [Inject] private IMemoryService _ltm { get; set; } = default!;
        [Inject] private DialogService _dialog { get; set; } = default!;
        [Inject] private ToastService _toasts { get; set; } = default!;

        private MemoryContextOptions _model = default!;

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
            var result = await _dialog.ShowModal("Clear Memory", $"Are you sure you want clear all long-term memories?", new ResultDialogOption()
            {
                Size = Size.Medium,
                ButtonYesText = "Yes",
                ButtonNoText = "No"
            });
            if(result == DialogResult.Yes)
            {
                await _ltm.ClearMemoryAsync();
                await _toasts.Information("Memories Erased", "Long-Term memory succesfully cleared");
            }
        }
    }
}
