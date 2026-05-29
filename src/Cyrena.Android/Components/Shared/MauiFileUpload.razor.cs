using Cyrena.Attributes;
using Cyrena.Contracts;
using Cyrena.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.SemanticKernel;
using MudBlazor;

namespace Cyrena.Android.Components.Shared
{
    public partial class MauiFileUpload
    {
        [KernelInject] private ConnectionInfo _info { get; set; } = default!;
        [KernelInject] private IFileHandlerFactory _factory { get; set; } = default!;
        [Inject] private ISnackbar _toasts { get; set; } = default!;

        private async Task AttachPhoto()
        {
            if (!_info.SupportImages) return;
            var status = await Permissions.RequestAsync<Permissions.Photos>();
            if (status != PermissionStatus.Granted)
            {
                _toasts.Add("Permission not granted", Severity.Error);
                return;
            }
            try
            {
                List<FileResult> photos = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions()
                {
                    SelectionLimit = 5
                });
                var models = new List<KernelContent>();
                foreach (var photo in photos)
                {
                    using var stream = await photo.OpenReadAsync();
                    var content = await _factory.SaveAsync(stream, photo.ContentType, photo.FileName);
                    if (content == null)
                        _toasts.Add($"{photo.FileName} not supported", Severity.Error);
                    else
                        models.Add(content);
                }
                await OnItemsAdded.InvokeAsync(models.ToArray());
            }catch (Exception ex)
            {
                _toasts.Add(ex.Message, Severity.Error);
            }
        }

        private async Task Capture()
        {
            var status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                _toasts.Add("Permission not granted", Severity.Error);
                return;
            }
            try
            {
                FileResult? photo = await MediaPicker.Default.CapturePhotoAsync();
                if(photo != null)
                {
                    using var stream = await photo.OpenReadAsync();
                    var content = await _factory.SaveAsync(stream, photo.ContentType, photo.FileName);
                    if (content == null)
                        _toasts.Add($"{photo.FileName} not supported", Severity.Error);
                    else
                        await OnItemsAdded.InvokeAsync([content]);
                }
            }
            catch (Exception ex)
            {
                _toasts.Add(ex.Message, Severity.Error);
            }
        }

        private async Task AttachFiles()
        {
            var status = await Permissions.RequestAsync<Permissions.StorageRead>();
            if (status != PermissionStatus.Granted)
            {
                _toasts.Add("Permission not granted", Severity.Error);
                return;
            }
            try
            {
                FileResult? result = await FilePicker.Default.PickAsync();
                if(result != null)
                {
                    using var stream = await result.OpenReadAsync();
                    var content = await _factory.SaveAsync(stream, result.ContentType, result.FileName);
                    if (content == null)
                        _toasts.Add($"{result.FileName} not supported", Severity.Error);
                    else
                        await OnItemsAdded.InvokeAsync([content]);
                }
            }
            catch (Exception ex)
            {
                _toasts.Add(ex.Message, Severity.Error);
            }
        }
    }
}
