using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Developer.Docs.Models;
using Cyrena.Extensions;
using Cyrena.Persistence.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;

namespace Cyrena.Developer.Docs.Components.Pages
{
    public partial class Index
    {
        [Inject] private IKernelController _kernels { get; set; } = default!;
        [Inject] private NavigationManager _nav { get; set;  } = default!;
        [Inject] private ToastService _toasts { get; set; } = default!;
        [Inject] private DialogService _dialog { get; set; } = default!;
        [Inject] private IFileDialog _files { get; set; } = default!;
        [Parameter] public string? KernelId { get; set; }

        private Kernel _kernel = default!;
        private IStore<ApiReference> _store = default!;
        private IEnumerable<ApiReference> _models = Enumerable.Empty<ApiReference>();
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            try
            {
                if (string.IsNullOrEmpty(KernelId))
                    throw new NullReferenceException("Kernel id not provided");

                var kernel = _kernels.GetKernel(KernelId);
                if (kernel == null)
                    throw new NullReferenceException("Unable to find instance of Kernel");
                _kernel = kernel;
                _store = _kernel.Services.GetRequiredService<IStore<ApiReference>>();
                _models = await _store.FindManyAsync(x => true);
                this.StateHasChanged();
            }
            catch (Exception ex)
            {
                await _toasts.Error("Error", ex.Message);
                _nav.NavigateTo("");
            }
        }

        private async Task Delete(ApiReference item)
        {
            var rf = await _dialog.ShowModal("Delete API Reference", $"Are you sure you want to delete '{item.Title}'?");
            if(rf == DialogResult.Yes)
            {
                await _store.DeleteAsync(item);
                _models = await _store.FindManyAsync(x => true);
                this.StateHasChanged();
            }
        }

        private async Task Export(ApiReference item)
        {
            try
            {
                var path = await _files.ShowSaveFile("Choose location", ($".aiapi", [".aiapi"]));
                if (string.IsNullOrEmpty(path)) return;
                if (!path.EndsWith(".aiapi"))
                    path += ".aiapi";
                File.WriteAllText(path, item.ToJson());

            }
            catch (Exception ex)
            {
                await _toasts.Error("Error", ex.Message);
            }
        }

        private async Task Import()
        {
            try
            {
                var path = await _files.OpenAsync("Choose file", ($".aiapi", [".aiapi"]));
                if (string.IsNullOrEmpty(path)) return;
                var json = File.ReadAllText(path);
                ApiReference? aiapi = JsonConvert.DeserializeObject<ApiReference>(json);
                if (aiapi == null) throw new NullReferenceException("Unable to deserialize");
                await _store.SaveAsync(aiapi);
                _models = await _store.FindManyAsync(x => true);
                this.StateHasChanged();
            }
            catch (Exception ex)
            {
                await _toasts.Error("Error", ex.Message);
            }
        }
    }
}
