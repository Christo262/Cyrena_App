namespace Cyrena.Contracts
{
    /// <summary>
    /// Primary service for performing imports.
    /// </summary>
    public interface IImportService
    {
        bool HasImporters();
        Task StartImportAsync(CancellationToken cancellationToken = default!);
    }
}
