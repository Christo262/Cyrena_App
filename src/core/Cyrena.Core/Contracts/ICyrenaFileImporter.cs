using Cyrena.Models;

namespace Cyrena.Contracts
{
    /// <summary>
    /// Handles the processing of a '.cyrena' imported file based on manifest. MUST BE GLOBAL SERVICE
    /// </summary>
    public interface ICyrenaFileImporter
    {
        string Id { get; }

        Task ImportAsync(CyrenaFileManifest manifest, string absoluteDataPath, CancellationToken cancellationToken = default);
    }
}
