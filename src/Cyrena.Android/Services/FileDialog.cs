using Cyrena.Contracts;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

namespace Cyrena.Android.Services
{
    internal class FileDialog : IFileDialog
    {
        public void ExploreFolder(string folderPath)
        {
            // Not realistically supported on Android like desktop.
        }

        public async Task<string?> OpenAsync(
            string title,
            (string filterName, string[] extensions)? filter)
        {
            if(filter.HasValue && (filter.Value.extensions.Contains(".png") || filter.Value.extensions.Contains(".jpg") || filter.Value.extensions.Contains(".jpeg")))
            {
                var results = await MainThread.InvokeOnMainThreadAsync(() => MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions()
                {
                    SelectionLimit = 1
                }));
                return results.FirstOrDefault()?.FullPath;
            }
            var options = new PickOptions
            {
                PickerTitle = title,
                FileTypes = CreateFileTypes(filter)
            };

            var result = await MainThread.InvokeOnMainThreadAsync(
                () => FilePicker.Default.PickAsync(options));

            return result?.FullPath;
        }

        public Task<string?> SelectFolder(
            string title = "Select Folder",
            string? current = null)
        {
            // Not supported by built-in MAUI FilePicker.
            return Task.FromResult<string?>(null);
        }

        public Task<string?> ShowSaveFileAsync(
            string title,
            (string filterName, string[] extensions)? filter,
            string? defaultPath = null)
        {
            // Not supported cleanly by built-in MAUI APIs.
            // Needs Android Storage Access Framework or CommunityToolkit.Maui FileSaver.
            return Task.FromResult<string?>(null);
        }

        private static FilePickerFileType? CreateFileTypes(
            (string filterName, string[] extensions)? filter)
        {
            if (filter is null)
                return null;

            var extensions = filter.Value.extensions
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().TrimStart('*').TrimStart('.'))
                .ToArray();

            if (extensions.Length == 0)
                return null;

            return new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    [DevicePlatform.Android] = extensions.Select(x => $".{x}")
                });
        }
    }
}