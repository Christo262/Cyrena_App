namespace Cyrena.Contracts
{
    /// <summary>
    /// Primary service for performing imports.
    /// </summary>
    public interface IImportService
    {
        Task StartImportAsync(CancellationToken cancellationToken = default!);
    }
}
