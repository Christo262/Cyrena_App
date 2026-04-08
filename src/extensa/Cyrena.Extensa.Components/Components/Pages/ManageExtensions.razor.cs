using BootstrapBlazor.Components;
using Cyrena.Extensa.Loader.Contracts;
using Cyrena.Extensa.Loader.Models;
using Cyrena.Extensa.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Cyrena.Extensa.Components.Pages
{
    public partial class ManageExtensions
    {
        [Inject] private IExtensionRegistry _registry { get; set; } = default!;
        [Inject] private IOptions<ExtensaOptions> _options { get; set; } = default!;
        [Inject] private DialogService _dialog { get; set; } = default!;
        [Inject] private ToastService _toasts { get; set;  } = default!;

        private List<string> _uninstalls { get; set; } = new List<string>();

        protected override void OnAfterRender(bool firstRender)
        {
            if (!firstRender) return;
            var path = Path.Combine(_options.Value.InstallationsDirectory, "uninstall.json");
            if(!File.Exists(path)) return;
            try
            {
                var json = File.ReadAllText(path);
                string[]? uns = JsonConvert.DeserializeObject<string[]>(json);
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

        private async Task MarkUninstall(LoadedExtension ext)
        {
            if (ext.Status == ExtensionStatus.Runtime)
                return;
            var rf = await _dialog.ShowModal("Uninstall Extension", $"Are you sure you want to uninstall {ext.Name} ({ext.Version})?", new ResultDialogOption()
            {
                Size = Size.Small
            });
            if(rf == DialogResult.Yes)
            {
                _uninstalls.Add(ext.Id);
                var path = Path.Combine(_options.Value.InstallationsDirectory, "uninstall.json");
                var json = JsonConvert.SerializeObject(_uninstalls.ToArray());
                File.WriteAllText(path, json);
                await _toasts.Success("Uninstall", $"{ext.Name} ({ext.Id}) will be uninstalled when the application starts.");
                this.StateHasChanged();
            }
        }

        private void CancelUninstall(string id)
        {
            if (_uninstalls.Contains(id))
            {
                _uninstalls.Remove(id);
                var path = Path.Combine(_options.Value.InstallationsDirectory, "uninstall.json");
                var json = JsonConvert.SerializeObject(_uninstalls.ToArray());
                File.WriteAllText(path, json);
                this.StateHasChanged();
            }
        }
    }
}
