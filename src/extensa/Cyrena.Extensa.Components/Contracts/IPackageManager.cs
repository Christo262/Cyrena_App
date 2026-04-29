using Cyrena.Extensa.Models;

namespace Cyrena.Extensa.Contracts
{
    public interface IPackageManager
    {
        IReadOnlyList<Exception> Errors { get; }
        PackageManagerStatus Status { get; }
        event EventHandler<PackageManagerStatus>? StatusChanged;
        Task IndexPackagesAsync(CancellationToken ct = default);
        Task<IEnumerable<Package>> ListPackagesAsync(CancellationToken ct = default);
        Task<Package?> GetPackageAsync(string id, CancellationToken ct = default);
        void ClearErrors();
        Task ClearCacheAsync();


        QueuedDownload? GetNextDownload();
        void ReportCurrentDownloadProgress(QueuedDownload? download, double progress);
        void EnqueueDownload(PluginServer server, string packageId, Version version);
        Task EnqueueDownloadAsync(Package item);
        bool IsQueued(string packageId);
    }
}
