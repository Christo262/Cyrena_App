using Cyrena.Contracts;
using Cyrena.Extensa.Contracts;
using Cyrena.Extensa.Models;
using Cyrena.Extensa.Options;
using Cyrena.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using MudBlazor;

namespace Cyrena.Extensa.Components.Shared
{
    public partial class Servers
    {
        [Inject] private IPluginServerService _service { get; set; } = default!;
        [Inject] private IDialogService _dialog { get; set; } = default!;
        [Inject] private IFileDialog _files { get; set;  } = default!;
        [Inject] private IOptions<ExtensaOptions> _options { get; set; } = default!;
        [Inject] private ISnackbar _snackbar { get; set; } = default!;
        [Inject] private ComponentOptions _ui { get; set; } = default!;

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
            var parameters = new DialogParameters<ServerForm> { { x => x.Model, model } };
            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small };
            var dialog = await _dialog.ShowAsync<ServerForm>("Add Extension Server", parameters, options);
            var result = await dialog.Result;
            if (result != null && !result.Canceled)
            {
                await _service.AddServerAsync(model);
                _models = await _service.GetAllServers();
                this.StateHasChanged();
            }
        }

        private async Task Edit(PluginServer model)
        {
            var parameters = new DialogParameters<ServerForm> { { x => x.Model, model } };
            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small };
            var dialog = await _dialog.ShowAsync<ServerForm>("Edit Extension Server", parameters, options);
            var result = await dialog.Result;
            if (result != null && !result.Canceled)
            {
                await _service.UpdateServerAsync(model);
                _models = await _service.GetAllServers();
                this.StateHasChanged();
            }
        }
        private async Task Delete(PluginServer model)
        {
            var parameters = new DialogParameters
            {
                { "ContentText", $"Are you sure you want to delete {model.Name}?" },
                { "ButtonText", "Delete" },
                { "CancelText", "Cancel" }
            };
            var options = new DialogOptions { CloseButton = true };
            var dialog = await _dialog.ShowAsync<MudMessageBox>("Delete Extension Server", parameters, options);
            var result = await dialog.Result;
            if(result != null && !result.Canceled)
            {
                await _service.RemoveServerAsync(model.Id);
                _models = await _service.GetAllServers();
                this.StateHasChanged();
            }
        }

        private async Task OpenExtensions()
        {
            try
            {
                _files.ExploreFolder(_options.Value.ExtensionsDirectory);
            }
            catch (Exception ex)
            {
                _snackbar.Add(ex.Message, Severity.Error);
            }
        }

        private async Task OpenInstallations()
        {
            try
            {
                _files.ExploreFolder(_options.Value.InstallationsDirectory);
            }
            catch (Exception ex)
            {
                _snackbar.Add(ex.Message, Severity.Error);
            }
        }
    }
}
