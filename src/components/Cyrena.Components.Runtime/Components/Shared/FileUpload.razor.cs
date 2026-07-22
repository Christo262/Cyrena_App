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
        [Inject] private ISnackbar _snackbar { get; set; } = default!;
        [KernelInject] private IFileHandlerFactory _factory { get; set; } = default!;
        private string? _accepts { get; set; }
        protected override void OnInitialized()
        {
            if (!_factory.HasFileHandlers) return;
            _accepts = string.Join(',', _factory.GetSupportedMimeTypes());
        }

        private async Task UploadFiles(IReadOnlyList<IBrowserFile> files)
        {
            var models = new List<KernelContent>();
            foreach (var file in files)
            {
                using var stream = file.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024); // 50MB limit
                var content = await _factory.SaveAsync(stream, file.ContentType, file.Name);
                if (content == null)
                    _snackbar.Add($"{file.Name}: File type is not supported in the current chat.", Severity.Error);
                else
                    models.Add(content);
            }
            if (models.Any())
                await OnItemsAdded.InvokeAsync(models.ToArray());
        }
    }
}
