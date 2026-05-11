using BootstrapBlazor.Components;
using Cyrena.Contracts;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using static System.Net.WebRequestMethods;

namespace Cyrena.HUD.Services
{
    internal class FileDialog : IFileDialog
    {
        public void ExploreFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                throw new NullReferenceException("Invalid folder path");
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer",
                Arguments = $"\"{folderPath}\"",
                UseShellExecute = true
            });
        }

        public Task<string?> OpenAsync(string title, (string filterName, string[] extensions)? ftr)
        {
            return Task.Run(() =>
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = title,
                };
                if (ftr.HasValue)
                {
                    var filter = $"{ftr.Value.filterName}|{string.Join(";", ftr.Value.extensions.Select(e => $"*{e}"))}";
                    dialog.Filter = filter;
                }
                bool? result = dialog.ShowDialog();
                return result == true ? dialog.FileName : null;
            });
        }

        public Task<string?> SelectFolder(string title = "Select Folder", string? current = null)
        {
            return Task.Run(() =>
            {
                var dialog = new Microsoft.Win32.OpenFolderDialog
                {
                    Title = title,
                    DefaultDirectory = current,  
                    Multiselect = false
                };
                bool? result = dialog.ShowDialog();
                return result == true ? dialog.FolderName : null;
            });
        }

        public Task<string?> ShowSaveFileAsync(string title, (string filterName, string[] extensions)? ftr, string? defaultPath = null)
        {
            return Task.Run(() =>
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = title,
                    FileName = defaultPath,
                };
                if (ftr.HasValue)
                {
                    var filter = $"{ftr.Value.filterName}|{string.Join(";", ftr.Value.extensions.Select(e => $"*{e}"))}";
                    dialog.Filter = filter;
                }
                bool? result = dialog.ShowDialog();
                return result == true ? dialog.FileName : null;
            });
        }
    }

}
