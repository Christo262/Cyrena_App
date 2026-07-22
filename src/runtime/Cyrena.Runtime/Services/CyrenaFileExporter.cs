using Cyrena.Contracts;
using Cyrena.Models;
using System.IO.Compression;
using System.Text.Json;

namespace Cyrena.Runtime.Services
{
    internal class CyrenaFileExporter : ICyrenaFileExporter
    {
        private readonly IChatConfigurationService _config;
        public CyrenaFileExporter(IChatConfigurationService config)
        {
            _config = config;
        }

        public async Task<CyrenaFileManifest> ExportFilesAsync(string extensionId, Version extensionVersion, string importerId, Dictionary<string, string?> properties, string outPath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(_config.Config.FileStoragePath))
                throw new ApplicationException("Configuration file storage path not set");
            if (!Directory.Exists(_config.Config.FileStoragePath) || Directory.GetFiles(_config.Config.FileStoragePath).Length == 0)
                throw new ApplicationException("Configuration file storage directory is empty or does not exist");
            if (!outPath.EndsWith(".cyrena"))
                throw new InvalidOperationException("Invalid output path");

            var manifest = new CyrenaFileManifest(extensionId, extensionVersion, importerId)
            {
                Properties = properties,
            };
            var json = JsonSerializer.Serialize(manifest);
            var manifestPath = Path.Combine(_config.Config.FileStoragePath, "cyrena.manifest.json");
            File.WriteAllText(manifestPath, json);
            
            try
            {
                if(File.Exists(outPath))
                    File.Delete(outPath);
                await ZipFile.CreateFromDirectoryAsync(_config.Config.FileStoragePath, outPath, cancellationToken);
                File.Delete(manifestPath);
                return manifest;
            }
            catch (Exception)
            {
                File.Delete(manifestPath);
                throw;
            }
        }
    }
}
