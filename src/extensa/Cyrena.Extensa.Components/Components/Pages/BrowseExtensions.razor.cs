using BootstrapBlazor.Components;
using Cyrena.Extensa.Contracts;
using Cyrena.Extensa.Loader.Contracts;
using Cyrena.Extensa.Models;
using Microsoft.AspNetCore.Components;

namespace Cyrena.Extensa.Components.Pages
{
    public partial class BrowseExtensions : IDisposable
    {
        [Inject] private IPackageManager _manager { get; set; } = default!;
        [Inject] private IPluginServerService _servers { get; set; } = default!;
        [Inject] private IExtensionRegistry _registry { get; set; } = default!;
        [Inject] private DialogService _dialog { get; set;  } = default!;
        private IEnumerable<Package> _models { get; set; } = Enumerable.Empty<Package>();
        private IEnumerable<PluginServer> _distros = Enumerable.Empty<PluginServer>();

        public void Dispose()
        {
            _manager.StatusChanged -= _manager_StatusChanged;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            _manager.StatusChanged += _manager_StatusChanged;
            _distros = await _servers.GetEnabledServers();
            _models = await _manager.ListPackagesAsync();
            if(_models.Count() == 0)
            {
                var rf = await _dialog.ShowModal("Index Packages", "Would you like to index packages now?", new ResultDialogOption()
                {
                    Size = Size.Small,
                });
                if(rf == DialogResult.Yes)
                {
                    _ = Task.Run(async () =>
                    {
                        await _manager.IndexPackagesAsync();
                        _models = await _manager.ListPackagesAsync();
                        await this.InvokeAsync(StateHasChanged);
                    });
                }
            }
            this.StateHasChanged();
        }

        private void _manager_StatusChanged(object? sender, PackageManagerStatus e)
        {
            this.InvokeAsync(StateHasChanged);
        }

        private void IndexPackages()
        {
            _ = Task.Run(async () =>
            {
                await _manager.IndexPackagesAsync();
                _models = await _manager.ListPackagesAsync();
                await this.InvokeAsync(StateHasChanged);
            });
        }

        private void ClearErrors()
        {
            _manager.ClearErrors();
            this.StateHasChanged();
        }

        private async Task ClearCache()
        {
            await _manager.ClearCacheAsync();
            _models = await _manager.ListPackagesAsync();
        }

        private bool CanDownload(string packageId)
        {
            if (_registry.Extensions.Any(f => f.Id == packageId)) return false;
            if (_manager.IsQueued(packageId)) return false;
            return true;
        }

        private bool CanUpdate(Package item)
        {
            var latest = item.Versions.OrderByDescending(item => item.Version).FirstOrDefault();
            var ext = _registry.Extensions.FirstOrDefault(x => x.Id == item.Id);
            if(latest == null || ext == null) return false;
            if (_manager.IsQueued(item.Id)) return false;
            return latest.Version > ext.Version;
        }

        private async Task QueueDownload(Package item)
        {
            await _manager.EnqueueDownloadAsync(item);
            this.StateHasChanged();
        }
    }
}
