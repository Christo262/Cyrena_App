using Cyrena.Extensa.Contracts;
using Cyrena.Extensa.Loader.Contracts;
using Cyrena.Extensa.Models;
using Cyrena.Extensa.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.IO.Compression;

namespace Cyrena.Extensa.Services
{
    internal class DownloadService : BackgroundService, IProgress<double>
    {
        private readonly IPackageManager _manager;
        private readonly IPluginPackageService _packages;
        private readonly ExtensaOptions _options;
        private readonly IExtensionRegistry _registry;
        private readonly ILogger<DownloadService> _logger;
        public DownloadService(IPackageManager manager, IPluginPackageService packages, IOptions<ExtensaOptions> options, IExtensionRegistry registry, ILogger<DownloadService> logger)
        {
            _manager = manager;
            _packages = packages;
            _options = options.Value;
            _logger = logger;
            _registry = registry;
        }

        private QueuedDownload? _current { get; set; }

        public void Report(double value)
        {
            _manager.ReportCurrentDownloadProgress(_current, value);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Download service running in background");
            while (!stoppingToken.IsCancellationRequested)
            {
                var next = _manager.GetNextDownload();
                if (next == null)
                {
                    await Task.Delay(500, stoppingToken);
                    continue;
                }

                try
                {
                    _current = next;
                    _manager.ReportCurrentDownloadProgress(_current, 0);
                    using var result = await _packages.DownloadPackageAsync(next.Server, next.PackageId, null, this, stoppingToken);
                    result.Stream.Position = 0;
                    var install = Path.Combine(_options.InstallationsDirectory, $"{next.PackageId}.zip");
                    var data = result.Stream.ToArray();
                    File.WriteAllBytes(install, data);
                    if (!next.IsUpdate)
                    {
                        var extension = Path.Combine(_options.ExtensionsDirectory, next.PackageId);
                        if (!Directory.Exists(extension))
                            Directory.CreateDirectory(extension);
                        ZipFile.ExtractToDirectory(install, extension);
                        File.Delete(install);
                        var manifest = Path.Combine(_options.ExtensionsDirectory, next.PackageId, _options.ExtensionInfoFileName);
                        if (!File.Exists(manifest))
                        {
                            Directory.Delete(extension, true);
                            throw new Exception($"{_options.ExtensionInfoFileName} missing from package");
                        }

                        var json = File.ReadAllText(manifest);
                        var info = JsonSerializer.Deserialize<ExtensionInfo>(json);
                        if (info == null)
                        {
                            Directory.Delete(extension, true);
                            throw new Exception($"Unable to deserialize manifest");
                        }

                        _registry.AddExtension(new Loader.Models.LoadedExtension()
                        {
                            Id = info.Id,
                            Dependencies = info.Dependencies,
                            Description = info.Description,
                            ContentRootDirectory = extension,
                            Name = info.Name,
                            Version = info.Version,
                            Status = Loader.Models.ExtensionStatus.Unloaded,
                            Errors = [new Exception("Please restart application to complete installation.")]
                        });

                        foreach (var item in info.Dependencies)
                        {
                            if (!_registry.Extensions.Any(x => x.Id == item.Id && x.Version >= item.MinVersion))
                                _manager.EnqueueDownload(next.Server, item.Id, item.MinVersion);
                        }
                    }
                    else
                    {
                        var ext = _registry.Extensions.FirstOrDefault(x => x.Id == next.PackageId);
                        if (ext != null)
                        {
                            ext.Version = next.Version;
                            ext.Errors.Add(new Exception("Please restart application to complete installation."));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, ex);
                }
                finally
                {
                    _manager.ReportCurrentDownloadProgress(null, 0);
                    await Task.Delay(500, stoppingToken);
                }
            }
        }
    }
}
