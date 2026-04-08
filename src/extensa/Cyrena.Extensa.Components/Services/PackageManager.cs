using Cyrena.Extensa.Contracts;
using Cyrena.Extensa.Loader.Contracts;
using Cyrena.Extensa.Models;
using Cyrena.Extensions;
using Cyrena.Persistence;
using Cyrena.Persistence.Contracts;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Cyrena.Extensa.Services
{
    internal class PackageManager : IPackageManager
    {
        private readonly IPluginServerService _servers;
        private readonly IPluginPackageService _packages;
        private readonly IExtensionRegistry _extensions;
        private readonly IStore<Package> _store;
        private readonly ILogger<PackageManager> _logger;
        public PackageManager(IPluginServerService servers, IPluginPackageService packages, IStore<Package> store, ILogger<PackageManager> logger, IExtensionRegistry extensions)
        {
            _servers = servers;
            _packages = packages;
            _store = store;
            _logger = logger;
            _extensions = extensions;
            _errors = new List<Exception>();
            Status = new PackageManagerStatus();
            _downloads = new ConcurrentQueue<QueuedDownload>();
        }

        private readonly List<Exception> _errors;
        public IReadOnlyList<Exception> Errors => _errors.AsReadOnly();

        public PackageManagerStatus Status { get; private set; }
        public event EventHandler<PackageManagerStatus>? StatusChanged;
    
        private readonly ConcurrentQueue<QueuedDownload> _downloads;
        private QueuedDownload? _current { get; set; }

        public async Task IndexPackagesAsync(CancellationToken ct = default)
        {
            if (Status.IsIndexing) return;
            SetIndexing(true);
            _logger.LogInformation("Indexing packages");
            var servers = await _servers.GetEnabledServers();
            foreach(var item in servers)
            {
                try
                {
                    var packages = await _packages.GetPackagesAsync(item, null, ct);
                    foreach(var package in packages)
                        await _store.SaveAsync(package);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex.Message, ex);
                    _errors.Add(ex);
                }
            }
            SetIndexing(false);
        }

        public async Task<IEnumerable<Package>> ListPackagesAsync(CancellationToken ct = default)
        {
            var packs = await _store.FindManyAsync(x => true, new OrderBy<Package>(x => x.Title, SortDirection.Ascending));
            return packs;
        }

        private void SetIndexing(bool isIndexing)
        {
            Status = new PackageManagerStatus(isIndexing, _downloads.Count, _current?.PackageId, _current?.Progress ?? 0);
            StatusChanged?.Invoke(this, Status);
        }

        public void ClearErrors()
        {
            _errors.Clear();
        }

        public async Task ClearCacheAsync()
        {
            await _store.DeleteManyAsync(x => true);
        }

        public QueuedDownload? GetNextDownload()
        {
            if(_downloads.TryDequeue(out var q))
                return q;
            
            return null;
        }

        public void ReportCurrentDownloadProgress(QueuedDownload? downoad, double progress)
        {
            _current = downoad;
            _current?.Progress = progress;
            var status = new PackageManagerStatus(Status.IsIndexing, _downloads.Count, _current?.PackageId, _current?.Progress ?? 0);
            Status = status;
            StatusChanged?.Invoke(this, Status);
        }

        public void EnqueueDownload(PluginServer server, string packageId)
        {
            if (_downloads.Any(x => x.PackageId == packageId))
                return;

            var q = new QueuedDownload()
            {
                PackageId = packageId,
                Server = server,
                IsUpdate = _extensions.Extensions.Any(x => x.Id == packageId)
            };
            if (!_downloads.Any(x => x.PackageId == packageId))
                _downloads.Enqueue(q);
            var status = new PackageManagerStatus(Status.IsIndexing, _downloads.Count, _current?.PackageId, _current?.Progress ?? 0);
            Status = status;
            StatusChanged?.Invoke(this, Status);
        }

        public async Task EnqueueDownloadAsync(Package item)
        {
            if (string.IsNullOrEmpty(item.ServerId)) return;
            var server = await _servers.GetServer(item.ServerId);
            if(server == null) return;
            EnqueueDownload(server, item.Id);
        }

        public bool IsQueued(string packageId)
        {
            if(_current?.PackageId == packageId) return true;
            if(_downloads.Any(x => x.PackageId == packageId))return true;
            return false;
        }
    }
}
