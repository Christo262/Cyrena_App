using Cyrena.Contracts;
using Microsoft.Win32;

namespace Cyrena.HUD.Services
{
    internal class FileDialog : IFileDialog
    {
        public Task<string?> OpenAsync(string title, (string filter, string[] types)? ftr)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = title,
            };
            if (ftr.HasValue)
            {
                
                var f = $"{ftr.Value.filter}({string.Join(";",ftr.Value.types.Select(x => $"*{x}"))})|{string.Join(";", ftr.Value.types.Select(x => $"*{x}"))}";
                dialog.Filter = f;
            }
            bool? result = dialog.ShowDialog();
            if (result == true)
                return Task.FromResult<string?>(dialog.FileName);
            return Task.FromResult<string?>(null);
        }

        public Task<string?> ShowSaveFile(string title, (string filter, string[] types)? ftr, string? defaultPath = null)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = title,
            };
            if(ftr.HasValue)
            {
                var f = $"{ftr.Value.filter}({string.Join(";", ftr.Value.types.Select(x => $"*{x}"))})|{string.Join(";", ftr.Value.types.Select(x => $"*{x}"))}";
                dialog.Filter = f;
            }
            bool? result = dialog.ShowDialog();
            if(result ==  true)
                return Task.FromResult<string?>(dialog.FileName);
            return Task.FromResult<string?>(null);
        }
    }
}
