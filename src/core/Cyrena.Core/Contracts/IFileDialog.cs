namespace Cyrena.Contracts
{
    public interface IFileDialog
    {
        Task<string?> OpenAsync(string title, (string filter, string[] types)? ftr);

        Task<string?> ShowSaveFile(string title, (string filter, string[] types)? ftr);
    }
}
