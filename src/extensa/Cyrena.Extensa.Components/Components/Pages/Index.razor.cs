using Cyrena.Extensa.Components.Shared;
using Cyrena.Extensa.Contracts;
using Cyrena.Extensa.Extensions;
using Cyrena.Extensa.Loader.Contracts;
using Cyrena.Extensa.Loader.Models;
using Cyrena.Extensa.Models;
using Cyrena.Extensa.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using MudBlazor;
using System.Text.Json;

namespace Cyrena.Extensa.Components.Pages
{
    public partial class Index : IDisposable
    {
        [Inject] private IExtensionRegistry _registry { get; set; } = default!;
        [Inject] private IOptions<ExtensaOptions> _options { get; set; } = default!;
        [Inject] private IPluginServerService _servers { get; set; } = default!;
        [Inject] private IDialogService _dialog { get; set; } = default!;
        [Inject] private ISnackbar _snackbar { get; set; } = default!;
        [Inject] private IPackageManager _manager { get; set; } = default!;

        private IEnumerable<PackageViewModel> _models { get; set; } = Enumerable.Empty<PackageViewModel>();
        private IEnumerable<PluginServer> _distros = Enumerable.Empty<PluginServer>();
        private List<string> _uninstalls { get; set; } = new List<string>();

        private string? _in_view { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            _manager.StatusChanged += _manager_StatusChanged;
            _distros = await _servers.GetEnabledServers();
            ReadUninstalls();
            var models = await _manager.ListPackagesAsync();
            if (models.Count() == 0)
            {
                var result = await _dialog.ShowMessageBoxAsync("Index Packages", "Would like to index available extensions?", "Yes", "No");
                if (result == true)
                    await RefreshPackages();
            }
            else
            {
                RebuildViewModel(models);
                this.StateHasChanged();
            }
        }

        private void RebuildViewModel(IEnumerable<Package> models)
        {
            var vms = new List<PackageViewModel>();
            foreach (var item in _registry.Extensions)
            {
                var package = models.FirstOrDefault(x => x.Id == item.Id);
                vms.Add(new PackageViewModel(package, item, _distros.FirstOrDefault(x => x.Id == package?.ServerId), _uninstalls));
            }
            foreach (var item in models)
            {
                var ext = vms.FirstOrDefault(x => x.Id == item.Id);
                if (ext == null)
                    vms.Add(new PackageViewModel(item, null, _distros.FirstOrDefault(x => x.Id == item.ServerId), _uninstalls));
            }
            _models = vms.OrderBy(x => x.Id);
        }

        private bool _is_refreshing { get; set; }
        private async Task RefreshPackages()
        {
            _is_refreshing = true;
            await _manager.ClearCacheAsync();
            RebuildViewModel(Enumerable.Empty<Package>());
            await _manager.IndexPackagesAsync();
            var models = await _manager.ListPackagesAsync();
            RebuildViewModel(models);
            await this.InvokeAsync(StateHasChanged);
            _is_refreshing = false;
        }

        private void _manager_StatusChanged(object? sender, PackageManagerStatus e)
        {
            this.InvokeAsync(() =>
            {
                RebuildViewModel(_models.Where(x => x.Package != null).Select(x => x.Package!));
                foreach (var item in _models)
                {
                    item.IsQueued = _manager.IsQueued(item.Id);
                    item.DownloadProgress = 0;
                    item.Downloading = false;
                }
                var ext = _models.FirstOrDefault(x => x.Id == e.CurrentDownload);
                if(ext != null)
                {
                    ext.Downloading = true;
                    ext.IsQueued = false;
                    ext.DownloadProgress = e.DownloadProgress;
                }
                StateHasChanged();
            });
        }

        public void Dispose()
        {
            _manager.StatusChanged -= _manager_StatusChanged;
        }

        private void ReadUninstalls()
        {
            var path = Path.Combine(_options.Value.InstallationsDirectory, "uninstall.json");
            if (!File.Exists(path)) return;
            try
            {
                var json = File.ReadAllText(path);
                string[]? uns = JsonSerializer.Deserialize<string[]>(json);
                if (uns == null)
                    throw new Exception("Unable to deserialize uninstall.json");
                _uninstalls.AddRange(uns);
                this.StateHasChanged();
            }
            catch (Exception ex)
            {
                File.Delete(path);
            }
        }

        private async Task QueueDownload(Package item)
        {
            await _manager.EnqueueDownloadAsync(item);
            RebuildViewModel(_models.Where(x => x.Package != null).Select(x => x.Package!));
            this.StateHasChanged();
        }

        private async Task MarkUninstall(LoadedExtension ext)
        {
            if (ext.Status == ExtensionStatus.Runtime)
                return;
            var parameters = new DialogParameters
            {
                { "ContentText", $"Are you sure you want to uninstall {ext.Name} ({ext.Version})?" },
                { "ButtonText", "Uninstall" },
                { "CancelText", "Cancel" }
            };
            var options = new DialogOptions { CloseButton = true };
            var dialog = await _dialog.ShowAsync<MudMessageBox>("Uninstall Extension", parameters, options);
            var result = await dialog.Result;
            if (result != null && !result.Canceled)
            {
                _uninstalls.Add(ext.Id);
                var path = Path.Combine(_options.Value.InstallationsDirectory, "uninstall.json");
                var json = JsonSerializer.Serialize(_uninstalls.ToArray());
                File.WriteAllText(path, json);
                _snackbar.Add($"{ext.Name} ({ext.Id}) will be uninstalled when the application starts.", Severity.Success);
                RebuildViewModel(_models.Where(x => x.Package != null).Select(x => x.Package!));
                this.StateHasChanged();
            }
        }

        private void CancelUninstall(string id)
        {
            if (_uninstalls.Contains(id))
            {
                _uninstalls.Remove(id);
                var path = Path.Combine(_options.Value.InstallationsDirectory, "uninstall.json");
                var json = JsonSerializer.Serialize(_uninstalls.ToArray());
                File.WriteAllText(path, json);
                RebuildViewModel(_models.Where(x => x.Package != null).Select(x => x.Package!));
                this.StateHasChanged();
            }
        }

        private bool _draw { get; set; }
        private async Task SetInView(PackageViewModel item)
        {
            _in_view = null;
            this.StateHasChanged();
            await Task.Delay(50);
            _in_view = item.Id;
            _draw = true;
            this.StateHasChanged();
        }

        private void CloseInView()
        {
            _in_view = null;
            _draw = false;
            this.StateHasChanged();
        }
    }

    internal record PackageViewModel(Package? Package, LoadedExtension? Extension, PluginServer? Server, List<string> Uninstalls)
    {
        public string Title
        {
            get
            {
                return Package?.Title ?? Extension?.Name ?? "Unknown";
            }
        }

        public string? Description
        {
            get
            {
                return Package?.Description ?? Extension?.Description;
            }
        }

        public Version Version
        {
            get
            {
                return Extension?.Version ?? Package?.LatestVersion().Version ?? new(0, 0, 1);
            }
        }

        public string Id
        {
            get
            {
                return Extension?.Id ?? Package?.Id ?? "unknown";
            }
        }

        public string? ServerId => Server?.Id ?? Package?.ServerId;

        public bool Installed => Extension != null;
        public bool HasUpdate
        {
            get
            {
                if (Extension == null || Package == null)
                    return false;
                var latest = Package.LatestVersion().Version;
                var installed = Extension.Version;
                return latest > installed;
            }
        }

        public bool WillUninstall
        {
            get
            {
                return Uninstalls.Contains(Id);
            }
        }

        public bool Downloading { get; set; }
        public double DownloadProgress { get; set; }
        public bool IsQueued { get; set; }
    }
}
