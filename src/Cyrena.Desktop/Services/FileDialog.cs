using Cyrena.Contracts;
using Photino.NET;
using System.Diagnostics;

namespace Cyrena.Desktop.Services
{
    internal class FileDialog : IFileDialog
    {
        private PhotinoWindow _window = default!;
        public FileDialog()
        {
        }

        internal void SetWindow(PhotinoWindow window)
        {
            _window = window;
        }

        public async Task<string?> OpenAsync(string title, (string filterName, string[] extensions)? ftr)
        {
            var ffs = await _window.ShowOpenFileAsync(title, null, false, ftr == null ? null : [ftr.Value]);
            return ffs.FirstOrDefault();
        }

        public async Task<string?> ShowSaveFileAsync(string title, (string filterName, string[] extensions)? ftr, string? defaultPath = null)
        {
            var output = await _window.ShowSaveFileAsync(title, defaultPath, ftr == null ? null : [ftr.Value]);
            return output;
        }

        public void ExploreFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                throw new NullReferenceException("Invalid folder path");

            // Photino runs on .NET, so we can just use Process.Start with the right command
            if (OperatingSystem.IsWindows())
            {
                // explorer.exe automatically opens the folder
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer",
                    Arguments = $"\"{folderPath}\"",
                    UseShellExecute = true
                });
            }
            else if (OperatingSystem.IsMacOS())
            {
                // macOS uses the `open` command
                Process.Start("open", $"\"{folderPath}\"");
            }
            else if (OperatingSystem.IsLinux())
            {
                // Most Linux desktops understand `xdg-open`
                Process.Start("xdg-open", $"\"{folderPath}\"");
            }
        }

        public async Task<string?> SelectFolder(string title = "Select Folder", string? current = null)
        {
            var output = await _window.ShowOpenFolderAsync(title, current, false);
            if(output.Length == 0)
                return null;
            return output[0];
        }
    }
}
