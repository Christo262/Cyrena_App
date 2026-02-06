using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Cyrena.Extensions;
using Cyrena.Persistence;
using Cyrena.Persistence.Contracts;
using Cyrena.Runtime.Ollama.Models;

namespace Cyrena.Runtime.Ollama.Components.Shared
{
    public partial class OllamaSettings
    {
        [Inject] private IStore<OllamaConnectionInfo> _store { get; set; } = default!;
        [Inject] private DialogService _dialog { get; set; } = default!;

        private IEnumerable<OllamaConnectionInfo>? _models { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            _models = await _store.FindManyAsync(x => true, new OrderBy<OllamaConnectionInfo>(x => x.Name, SortDirection.Ascending));
            this.StateHasChanged();
        }

        private async Task Create()
        {
            var model = new OllamaConnectionInfo();
            var rf = await _dialog.ShowModal<OllamaConnectionForm>(new ResultDialogOption()
            {
                Title = "Add Ollama Connection",
                Size = Size.Medium,
                ButtonYesText = "Submit",
                ButtonNoText = "Cancel",
                ComponentParameters = new()
                {
                    {"Model", model }
                }
            });
            if(rf == DialogResult.Yes)
            {
                await _store.AddAsync(model);
                _models = await _store.FindManyAsync(x => true, new OrderBy<OllamaConnectionInfo>(x => x.Name, SortDirection.Ascending));
                this.StateHasChanged();
            }
        }

        private async Task Edit(OllamaConnectionInfo model)
        {
            var rf = await _dialog.ShowModal<OllamaConnectionForm>(new ResultDialogOption()
            {
                Title = "Edit Ollama Connection",
                Size = Size.Medium,
                ButtonYesText = "Submit",
                ButtonNoText = "Cancel",
                ComponentParameters = new()
                {
                    {"Model", model }
                }
            });
            if (rf == DialogResult.Yes)
            {
                await _store.UpdateAsync(model);
                _models = await _store.FindManyAsync(x => true, new OrderBy<OllamaConnectionInfo>(x => x.Name, SortDirection.Ascending));
                this.StateHasChanged();
            }
        }

        private async Task DeleteAsync(OllamaConnectionInfo model)
        {
            var res = await _dialog.ShowModal("Delete Ollama Connection", $"Are you sure you want to delete {model.Name}?");
            if(res == DialogResult.Yes)
            {
                await _store.DeleteAsync(model);
                _models = await _store.FindManyAsync(x => true, new OrderBy<OllamaConnectionInfo>(x => x.Name, SortDirection.Ascending));
                this.StateHasChanged();
            }
        }
    }
}
