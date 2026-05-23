using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Cyrena.Contracts;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Cyrena.Shell.Services
{
    internal class FileDialog : IFileDialog
    {
        private Window _window = default!;

        public FileDialog()
        {
        }

        internal void SetWindow(Window window)
        {
            _window = window;
        }

        public void ExploreFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath)) return;

            Process.Start(new ProcessStartInfo
            {
                FileName = folderPath,
                UseShellExecute = true
            });
        }

        public async Task<string?> OpenAsync(string title, (string filterName, string[] extensions)? filter)
        {
            var options = new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = filter is null ? null : new[]
                {
                    new FilePickerFileType(filter.Value.filterName)
                    {
                        Patterns = filter.Value.extensions.Select(e => $"*.{e}").ToArray()
                    }
                }
            };

            var result = await _window.StorageProvider.OpenFilePickerAsync(options);
            return result?.FirstOrDefault()?.TryGetLocalPath();
        }

        public async Task<string?> SelectFolder(string title = "Select Folder", string? current = null)
        {
            var options = new FolderPickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                SuggestedStartLocation = current is null ? null
                    : await _window.StorageProvider.TryGetFolderFromPathAsync(current)
            };

            var result = await _window.StorageProvider.OpenFolderPickerAsync(options);
            return result?.FirstOrDefault()?.TryGetLocalPath();
        }

        public async Task<string?> ShowSaveFileAsync(string title, (string filterName, string[] extensions)? filter, string? defaultPath = null)
        {
            var options = new FilePickerSaveOptions
            {
                Title = title,
                SuggestedFileName = defaultPath is null ? null : Path.GetFileName(defaultPath),
                SuggestedStartLocation = defaultPath is null ? null
                    : await _window.StorageProvider.TryGetFolderFromPathAsync(Path.GetDirectoryName(defaultPath)!),
                FileTypeChoices = filter is null ? null : new[]
                {
                    new FilePickerFileType(filter.Value.filterName)
                    {
                        Patterns = filter.Value.extensions.Select(e => $"*.{e}").ToArray()
                    }
                }
            };

            var result = await _window.StorageProvider.SaveFilePickerAsync(options);
            return result?.TryGetLocalPath();
        }
    }
}