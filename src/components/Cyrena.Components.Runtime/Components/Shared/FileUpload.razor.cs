using Cyrena.Attributes;
using Cyrena.Contracts;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Microsoft.SemanticKernel;
using MudBlazor;

namespace Cyrena.Components.Shared
{
    public partial class FileUpload
    {
        [Parameter]
        public EventCallback<KernelContent[]> OnItemsAdded { get; set; }
        [Inject] private IJSRuntime _js { get; set; } = default!;
        [Inject] private ISnackbar _snackbar { get; set; } = default!;
        [KernelInject] private IFileHandlerFactory _factory { get; set; } = default!;
        private string? _accepts { get; set; }
        protected override void OnInitialized()
        {
            if (!_factory.HasFileHandlers) return;
            _accepts = string.Join(',', _factory.GetSupportedMimeTypes());
        }

        private async Task TriggerFileUpload()
        {
            await _js.InvokeVoidAsync("triggerClick", _fileInput.Element);
        }

        private InputFile _fileInput = null!;

        private async Task HandleFilesSelected(InputFileChangeEventArgs e)
        {
            var files = e.GetMultipleFiles(maximumFileCount: 10);
            List<KernelContent> models = new List<KernelContent>();

            foreach (var file in files)
            {
                try
                {
                    using var stream = file.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024); // 50MB limit
                    var content = await _factory.SaveAsync(stream, file.ContentType, file.Name);
                    if (content == null)
                        _snackbar.Add($"{file.Name}: File type is not supported in the current chat.", Severity.Error);
                    else
                        models.Add(content);
                }catch (Exception ex)
                {
                    _snackbar.Add($"{file.Name}: {ex.Message}", Severity.Error);
                }
            }

            if (models.Count > 0)
                await OnItemsAdded.InvokeAsync(models.ToArray());

            StateHasChanged();
        }
    }
}
