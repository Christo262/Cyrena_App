using Microsoft.AspNetCore.Components;
using MudBlazor;
using Cyrena.Extensions;
using Cyrena.Persistence;
using Cyrena.Persistence.Contracts;
using Cyrena.Runtime.Ollama.Models;

namespace Cyrena.Runtime.Ollama.Components.Shared
{
    public partial class OllamaSettings
    {
        [Inject] private IStore<OllamaConnectionInfo> _store { get; set; } = default!;
        [Inject] private IDialogService _dialog { get; set; } = default!;

        private IEnumerable<OllamaConnectionInfo> _models { get; set; } = Enumerable.Empty<OllamaConnectionInfo>();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            _models = await _store.FindManyAsync(x => true, new OrderBy<OllamaConnectionInfo>(x => x.Name, Persistence.SortDirection.Ascending));
            this.StateHasChanged();
        }

        private async Task Create()
        {
            var model = new OllamaConnectionInfo();
            var parameters = new DialogParameters<OllamaConnectionForm>
            {
                { x => x.Model, model }
            };
            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
            var dialog = await _dialog.ShowAsync<OllamaConnectionForm>("Add Ollama Connection", parameters, options);
            var result = await dialog.Result;
            if (result is { Canceled: false })
            {
                await _store.AddAsync(model);
                _models = await _store.FindManyAsync(x => true, new OrderBy<OllamaConnectionInfo>(x => x.Name, Persistence.SortDirection.Ascending));
                this.StateHasChanged();
            }
        }

        private async Task Edit(OllamaConnectionInfo model)
        {
            var parameters = new DialogParameters<OllamaConnectionForm>
            {
                { x => x.Model, model }
            };
            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
            var dialog = await _dialog.ShowAsync<OllamaConnectionForm>("Edit Ollama Connection", parameters, options);
            var result = await dialog.Result;
            if (result is { Canceled: false })
            {
                await _store.UpdateAsync(model);
                _models = await _store.FindManyAsync(x => true, new OrderBy<OllamaConnectionInfo>(x => x.Name, Persistence.SortDirection.Ascending));
                this.StateHasChanged();
            }
        }

        private async Task DeleteAsync(OllamaConnectionInfo model)
        {
            var parameters = new DialogParameters<MudMessageBox>
            {
                { x => x.Title, "Delete Ollama Connection" },
                { x => x.Message, $"Are you sure you want to delete {model.Name}?" },
                { x => x.YesText, "Delete" },
                { x => x.CancelText, "Cancel" }
            };
            var dialog = await _dialog.ShowAsync<MudMessageBox>("", parameters);
            var result = await dialog.Result;
            if (result is { Canceled: false })
            {
                await _store.DeleteAsync(model);
                _models = await _store.FindManyAsync(x => true, new OrderBy<OllamaConnectionInfo>(x => x.Name, Persistence.SortDirection.Ascending));
                this.StateHasChanged();
            }
        }
    }
}