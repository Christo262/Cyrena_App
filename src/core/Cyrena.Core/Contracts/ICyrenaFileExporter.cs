using Cyrena.Models;

namespace Cyrena.Contracts
{
    /// <summary>
    /// Exports conversation files as a '.cyrena' zipped archive with manifest info to process such files. Kernel Locked
    /// </summary>
    public interface ICyrenaFileExporter
    {
        Task<CyrenaFileManifest> ExportFilesAsync(string extensionId, Version extensionVersion, string importerId, Dictionary<string, string?> properties, string outPath, CancellationToken cancellationToken = default);
    }
}
