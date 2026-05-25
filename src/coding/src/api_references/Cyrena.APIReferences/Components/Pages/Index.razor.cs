using Cyrena.Contracts;
using Cyrena.APIReferences.Models;
using Cyrena.Extensions;
using Cyrena.Persistence.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using System.Text.Json;
using Cyrena.Persistence;
using MudBlazor;

namespace Cyrena.APIReferences.Components.Pages
{
    public partial class Index
    {
        [Inject] private IKernelController _kernels { get; set; } = default!;
        [Inject] private NavigationManager _nav { get; set;  } = default!;
        [Inject] private ISnackbar _snackbar { get; set; } = default!;
        [Inject] private IDialogService _dialogService { get; set; } = default!;
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
                _models = await _store.FindManyAsync(x => true, new OrderBy<ApiReference>(x => x.Title, Cyrena.Persistence.SortDirection.Ascending));
                this.StateHasChanged();
            }
            catch (Exception ex)
            {
                _snackbar.Add(ex.Message, Severity.Error);
                _nav.NavigateTo("");
            }
        }

        private async Task Delete(ApiReference item)
        {
            var result = await _dialogService.ShowMessageBoxAsync(
                "Delete API Reference",
                $"Are you sure you want to delete '{item.Title}'?",
                yesText: "Delete",
                cancelText: "Cancel");
            if(result == true)
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
                var path = await _files.ShowSaveFileAsync("Choose location", ($".aiapi", [".aiapi"]));
                if (string.IsNullOrEmpty(path)) return;
                if (!path.EndsWith(".aiapi"))
                    path += ".aiapi";
                File.WriteAllText(path, JsonSerializer.Serialize(item));

            }
            catch (Exception ex)
            {
                _snackbar.Add(ex.Message, Severity.Error);
            }
        }

        private async Task Import()
        {
            try
            {
                var path = await _files.OpenAsync("Choose file", ($".aiapi", [".aiapi"]));
                if (string.IsNullOrEmpty(path)) return;
                var json = File.ReadAllText(path);
                ApiReference? aiapi = JsonSerializer.Deserialize<ApiReference>(json);
                if (aiapi == null) throw new NullReferenceException("Unable to deserialize");
                await _store.SaveAsync(aiapi);
                _models = await _store.FindManyAsync(x => true);
                this.StateHasChanged();
            }
            catch (Exception ex)
            {
                _snackbar.Add(ex.Message, Severity.Error);
            }
        }

        private void Back()
        {
            _nav.NavigateTo("");
        }
    }
}
