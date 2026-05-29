using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Options;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace Cyrena.Runtime.Services
{
    internal class ImportService : IImportService
    {
        private readonly IFileDialog _files;
        private readonly IServiceProvider _services;
        public ImportService(IFileDialog files, IServiceProvider services)
        {
            _files = files;
            _services = services;
        }

        public async Task StartImportAsync(CancellationToken cancellationToken = default)
        {
            string? dest = null;
            try
            {
                var path = await _files.OpenAsync("Select .cyrena file", (".cyrena", [".cyrena"]));
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    return;
                var tmp = Path.Combine(CyrenaBuilder.AppDataDirectory, "temp");
                if (!Directory.Exists(tmp))
                    Directory.CreateDirectory(tmp);
                dest = Path.Combine(tmp, Ulid.NewUlid().ToString());
                ZipFile.ExtractToDirectory(path, dest);
                var manifestPath = Path.Combine(dest, "cyrena.manifest.json");
                if (!File.Exists(manifestPath))
                {
                    Directory.Delete(dest, true);
                    throw new Exception("Unable to find manifest for import");
                }

                var json = File.ReadAllText(manifestPath);
                var manifest = JsonSerializer.Deserialize<CyrenaFileManifest>(json);
                if (manifest == null)
                    throw new Exception("Unable to deserialize manifest");
                var importers = _services.GetServices<ICyrenaFileImporter>();
                var importer = importers.FirstOrDefault(x => x.Id == manifest.ImporterId);
                if (importer == null)
                    throw new Exception($"Unable to find importer. This file requires {manifest.Extension} with min version {manifest.Version}");
                await importer.ImportAsync(manifest, dest, cancellationToken);
            }
            catch (Exception)
            {
                if(!string.IsNullOrEmpty(dest) && Directory.Exists(dest))
                    Directory.Delete(dest, true);
                throw;
            }
        }
    }
}
