using Cyrena.Contracts;

namespace Cyrena.Mobile.Services
{
    using Microsoft.Maui.Storage;

    internal class FileDialog : IFileDialog
    {
        public async Task<string?> OpenAsync(string title, (string filterName, string[] extensions)? ftr)
        {
            var options = new PickOptions
            {
                PickerTitle = title,
            };
            if (ftr.HasValue)
            {
                options.FileTypes = new FilePickerFileType(
                    new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                    { DevicePlatform.WinUI, ftr.Value.extensions.Select(e => $"*{e}") },
                    { DevicePlatform.Android, ftr.Value.extensions },
                    { DevicePlatform.iOS, ftr.Value.extensions },
                    { DevicePlatform.MacCatalyst, ftr.Value.extensions },
                    }
                );
            }
            var result = await FilePicker.Default.PickAsync(options);
            return result?.FullPath;
        }

        public async Task<string?> ShowSaveFileAsync(string title, (string filterName, string[] extensions)? ftr, string? defaultPath = null)
        {
            var options = new PickOptions
            {
                PickerTitle = title,
            };
            if (ftr.HasValue)
            {
                options.FileTypes = new FilePickerFileType(
                    new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                    { DevicePlatform.WinUI, ftr.Value.extensions.Select(e => $"*{e}") },
                    { DevicePlatform.Android, ftr.Value.extensions },
                    { DevicePlatform.iOS, ftr.Value.extensions },
                    { DevicePlatform.MacCatalyst, ftr.Value.extensions },
                    }
                );
            }
            var result = await FilePicker.Default.PickAsync(options);
            return result?.FullPath;
        }
    }

}
