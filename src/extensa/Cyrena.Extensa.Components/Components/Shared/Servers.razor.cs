using BootstrapBlazor.Components;
using Cyrena.Extensa.Contracts;
using Cyrena.Extensa.Models;
using Microsoft.AspNetCore.Components;

namespace Cyrena.Extensa.Components.Shared
{
    public partial class Servers
    {
        [Inject] private IPluginServerService _service { get; set; } = default!;
        [Inject] private DialogService _dialog { get; set; } = default!;

        private IEnumerable<PluginServer> _models = Enumerable.Empty<PluginServer>();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            _models = await _service.GetAllServers();
            this.StateHasChanged();
        }

        private async Task OnServerEnableChange(PluginServer server, bool enabled)
        {
            await _service.SetServerEnabledAsync(server.Id, enabled);
        }

        private async Task Add()
        {
            var model = new PluginServer();
            var result = await _dialog.ShowModal<ServerForm>(new ResultDialogOption()
            {
                Title = "Add Extension Server",
                Size = Size.Medium,
                ButtonYesText = "Submit",
                ButtonNoText = "Cancel",
                ComponentParameters = new()
                {
                    {"Model", model }
                }
            });
            if(result == DialogResult.Yes)
            {
                await _service.AddServerAsync(model);
                _models = await _service.GetAllServers();
                this.StateHasChanged();
            }
        }

        private async Task Edit(PluginServer model)
        {
            var result = await _dialog.ShowModal<ServerForm>(new ResultDialogOption()
            {
                Title = "Edit Extension Server",
                Size = Size.Medium,
                ButtonYesText = "Submit",
                ButtonNoText = "Cancel",
                ComponentParameters = new()
                {
                    {"Model", model }
                }
            });
            if (result == DialogResult.Yes)
            {
                await _service.UpdateServerAsync(model);
                _models = await _service.GetAllServers();
                this.StateHasChanged();
            }
        }

        private async Task Delete(PluginServer model)
        {
            var result = await _dialog.ShowModal("Delete Extension Server", $"Are you sure you want to delete {model.Name}?", new ResultDialogOption()
            {
                Size = Size.Medium,
            });
            if(result == DialogResult.Yes)
            {
                await _service.RemoveServerAsync(model.Id);
                _models = await _service.GetAllServers();
                this.StateHasChanged();
            }
        }
    }
}
