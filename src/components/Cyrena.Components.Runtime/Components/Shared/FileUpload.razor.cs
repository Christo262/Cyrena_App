using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Microsoft.SemanticKernel;

namespace Cyrena.Components.Shared
{
    public partial class FileUpload
    {
        [Parameter]
        public EventCallback<AdditionalMessageContent[]> OnItemsAdded { get; set; }
        [Inject] private IJSRuntime _js { get; set; } = default!;
        [Inject] private ToastService _toasts { get; set; } = default!;
        private ConnectionInfo _info = default!;
        private string? _accepts { get; set; }
        protected override void OnInitialized()
        {
            _info = Kernel.Services.GetRequiredService<ConnectionInfo>();
            var handlers = Kernel.Services.GetServices<IFileHandler>();
            if (handlers.Count() == 0) return;
            var mimes = new List<string>();
            foreach (var handler in handlers)
                mimes.AddRange(handler.GetSupportedMimeTypes());
            _accepts = string.Join(',', mimes.ToArray());
        }

        private async Task TriggerFileUpload()
        {
            await _js.InvokeVoidAsync("triggerClick", _fileInput.Element);
        }

        private InputFile _fileInput = null!;

        private async Task HandleFilesSelected(InputFileChangeEventArgs e)
        {
            var files = e.GetMultipleFiles(maximumFileCount: 10);
            List<AdditionalMessageContent> models = new List<AdditionalMessageContent>();

            foreach (var file in files)
            {
                try
                {
                    using var stream = file.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024); // 50MB limit
                    var handlers = Kernel.Services.GetServices<IFileHandler>();
                    AdditionalMessageContent? content = null;
                    foreach (var handler in handlers)
                    {
                        if(handler.HandlesType(file.ContentType, file.Name))
                        {
                            content = await handler.GetMessageContent(stream, file.ContentType, file.Name);
                            if (content != null)
                                break;
                        }
                    }
                    if (content == null)
                        throw new Exception("File type is not supported");
                    models.Add(content);
                }catch (Exception ex)
                {
                    await _toasts.Error($"{file.Name} Error", ex.Message);
                }
            }

            if (models.Count > 0)
                await OnItemsAdded.InvokeAsync(models.ToArray());

            StateHasChanged();
        }
    }
}
