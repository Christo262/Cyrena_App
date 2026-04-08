using Cyrena.Contracts;
using Microsoft.Win32;

namespace Cyrena.HUD.Services
{
    internal class FileDialog : IFileDialog
    {
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
