using Cyrena.Extensa.Installer.Contracts;
using Cyrena.Extensa.Installer.Models;
using Cyrena.Extensa.Loader.Contracts;
using Cyrena.Extensa.Loader.Models;
using Cyrena.Extensa.Models;
using Cyrena.Extensa.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.IO.Compression;

namespace Cyrena.Extensa.Installer.Services
{
    internal class InstallerService : IInstaller
    {
        private readonly IExtensionRegistry _registry;
        private readonly ExtensaOptions _options;
        private readonly ILogger<InstallerService> _logger;
        public InstallerService(IExtensionRegistry registry, IOptions<ExtensaOptions> options, ILogger<InstallerService> logger)
        {
            _registry = registry;
            _options = options.Value;
            _logger = logger;
        }

        public InstallResult Install(string file)
        {
            var result = new InstallResult() { File = file };
            var path = Path.Combine(_options.InstallationsDirectory, file);
            try
            {
                _logger.LogInformation($"Starting installation of {file}");
                
                if (!File.Exists(path))
                    throw new Exception($"{path} does not exist");
                var info = new FileInfo(path);
                var destinationDirectory = Path.Combine(_options.ExtensionsDirectory, info.Name.Replace(".zip", ""));
                LoadedExtension? loadedExtension = null;
                if(Directory.Exists(destinationDirectory))
                {
                    var infoFile = Path.Combine(destinationDirectory, _options.ExtensionInfoFileName);
                    if (File.Exists(infoFile))
                    {
                        var json = File.ReadAllText(infoFile);
                        var extensionInfo = JsonConvert.DeserializeObject<ExtensionInfo>(json);
                        if (extensionInfo != null)
                        {
                            loadedExtension = _registry.Extensions.FirstOrDefault(x => x.Id == extensionInfo.Id);
                            if (extensionInfo.RequireFrameworkBuilder || loadedExtension != null)
                            {
                                result.RequireRestart = true;
                                return result;
                            }
                        }
                    }
                    Directory.Delete(destinationDirectory, true);
                }
                ZipFile.ExtractToDirectory(path, destinationDirectory);

                var newExtensionInfo = Path.Combine(destinationDirectory, _options.ExtensionInfoFileName);
                _logger.LogInformation($"Looking for info file {newExtensionInfo}...");
                if (File.Exists(newExtensionInfo))
                {
                    var json = File.ReadAllText(newExtensionInfo);
                    var extensionInfo = JsonConvert.DeserializeObject<ExtensionInfo>(json);
                    if (loadedExtension == null)
                    {
                        loadedExtension = new LoadedExtension();
                        _registry.AddExtension(loadedExtension);
                    }
                    if (extensionInfo != null)
                    {
                        loadedExtension.Id = extensionInfo.Id;
                        loadedExtension.Icon = extensionInfo.Icon;
                        loadedExtension.Status = ExtensionStatus.Unloaded;
                        loadedExtension.Name = extensionInfo.Name;
                        loadedExtension.Description = extensionInfo.Description;
                        loadedExtension.Version = extensionInfo.Version;
                        loadedExtension.ContentRootDirectory = extensionInfo.ContentRootDirectory;
                        loadedExtension.Dependencies = extensionInfo.Dependencies;
                        loadedExtension.RequireFrameworkBuilder = extensionInfo.RequireFrameworkBuilder;
                        if(extensionInfo.RequireFrameworkBuilder)
                            result.RequireRestart = true;
                    }
                }
                else
                    throw new InvalidOperationException($"The extension info file, {file} does not exist");
                _logger.LogInformation($"{file} installed successfully");
                result.Success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                result.Errors.Add(ex);
            }
            if (File.Exists(path))
                File.Delete(path);
            return result;
        }
    }
}
