using Cyrena.Contracts;
using Photino.NET;

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

        public async Task<string?> OpenAsync(string title, (string filter, string[] types)? ftr)
        {
            var ffs = await _window.ShowOpenFileAsync(title, null, false, ftr == null ? null : [ftr.Value]);
            return ffs.FirstOrDefault();
        }

        public async Task<string?> ShowSaveFile(string title, (string filter, string[] types)? ftr, string? defaultPath = null)
        {
            var output = await _window.ShowSaveFileAsync(title, defaultPath, ftr == null ? null : [ftr.Value]);
            return output;
        }
    }
}
