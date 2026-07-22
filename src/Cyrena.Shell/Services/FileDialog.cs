using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Cyrena.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cyrena.Shell.Services
{
    internal class FileDialog : IFileDialog
    {
        private readonly SemaphoreSlim _dialogLock = new(1, 1);
        private Window? _window;

        internal void SetWindow(Window window)
        {
            _window = window;
        }

        public void ExploreFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                return;

            if (!Directory.Exists(folderPath))
                return;

            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"\"{folderPath}\"",
                    UseShellExecute = true
                });

                return;
            }

            if (OperatingSystem.IsMacOS())
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "open",
                    ArgumentList = { folderPath },
                    UseShellExecute = false
                });

                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = "xdg-open",
                ArgumentList = { folderPath },
                UseShellExecute = false
            });
        }

        public Task<string?> OpenAsync(
            string title,
            (string filterName, string[] extensions)? filter)
        {
            return RunDialogAsync(async window =>
            {
                var files = await window.StorageProvider.OpenFilePickerAsync(
                    new FilePickerOpenOptions
                    {
                        Title = title,
                        AllowMultiple = false,
                        FileTypeFilter = CreateFileTypes(filter)
                    });

                return files.FirstOrDefault()?.Path.LocalPath;
            });
        }

        public Task<string?> SelectFolder(
            string title = "Select Folder",
            string? current = null)
        {
            return RunDialogAsync(async window =>
            {
                var folders = await window.StorageProvider.OpenFolderPickerAsync(
                    new FolderPickerOpenOptions
                    {
                        Title = title,
                        AllowMultiple = false,
                        SuggestedStartLocation = await TryGetFolderAsync(window, current)
                    });

                return folders.FirstOrDefault()?.Path.LocalPath;
            });
        }

        public Task<string?> ShowSaveFileAsync(
            string title,
            (string filterName, string[] extensions)? filter,
            string? defaultPath = null)
        {
            return RunDialogAsync(async window =>
            {
                var file = await window.StorageProvider.SaveFilePickerAsync(
                    new FilePickerSaveOptions
                    {
                        Title = title,
                        FileTypeChoices = CreateFileTypes(filter),
                        SuggestedFileName = GetSuggestedFileName(defaultPath),
                        SuggestedStartLocation = await TryGetFolderAsync(window, GetFolderPath(defaultPath)),
                        DefaultExtension = GetDefaultExtension(filter)
                    });

                return file?.Path.LocalPath;
            });
        }

        private async Task<string?> RunDialogAsync(Func<Window, Task<string?>> action)
        {
            await _dialogLock.WaitAsync();

            try
            {
                if (Dispatcher.UIThread.CheckAccess())
                    return await action(GetWindow());

                var task = await Dispatcher.UIThread.InvokeAsync(
                    () => action(GetWindow()));

                return task;
            }
            finally
            {
                _dialogLock.Release();
            }
        }

        private Window GetWindow()
        {
            return _window
                ?? throw new InvalidOperationException("FileDialog window has not been set.");
        }

        private static async Task<IStorageFolder?> TryGetFolderAsync(Window window, string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            if (!Directory.Exists(path))
                return null;

            return await window.StorageProvider.TryGetFolderFromPathAsync(path);
        }

        private static IReadOnlyList<FilePickerFileType>? CreateFileTypes(
            (string filterName, string[] extensions)? filter)
        {
            if (filter is null)
                return null;

            var patterns = filter.Value.extensions
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(NormalizeExtensionPattern)
                .ToArray();

            if (patterns.Length == 0)
                return null;

            return
            [
                new FilePickerFileType(filter.Value.filterName)
                {
                    Patterns = patterns
                }
            ];
        }

        private static string NormalizeExtensionPattern(string extension)
        {
            extension = extension.Trim();

            if (extension == "*")
                return "*";

            if (extension.StartsWith("*."))
                return extension;

            if (extension.StartsWith('.'))
                return $"*{extension}";

            return $"*.{extension}";
        }

        private static string? GetFolderPath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            if (Directory.Exists(path))
                return path;

            return Path.GetDirectoryName(path);
        }

        private static string? GetSuggestedFileName(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            if (Directory.Exists(path))
                return null;

            return Path.GetFileName(path);
        }

        private static string? GetDefaultExtension(
            (string filterName, string[] extensions)? filter)
        {
            var extension = filter?.extensions.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(extension))
                return null;

            extension = extension.Trim();

            if (extension == "*")
                return null;

            if (extension.StartsWith("*."))
                return extension[2..];

            if (extension.StartsWith('.'))
                return extension[1..];

            return extension;
        }
    }
}