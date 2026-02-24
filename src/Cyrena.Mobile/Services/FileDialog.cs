using Cyrena.Contracts;

namespace Cyrena.Mobile.Services
{
#warning TODO: MAUI file save APIs different
    internal class FileDialog : IFileDialog
    {
        public Task<string?> OpenAsync(string title, (string filter, string[] types)? ftr)
        {
            throw new NotImplementedException();
        }

        public Task<string?> ShowSaveFile(string title, (string filter, string[] types)? ftr, string? defaultPath = null)
        {
            throw new NotImplementedException();
        }
    }
}
