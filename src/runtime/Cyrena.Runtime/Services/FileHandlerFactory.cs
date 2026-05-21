using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Persistence.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using System.Text.Json;

namespace Cyrena.Runtime.Services
{
    internal class FileHandlerFactory : IFileHandlerFactory
    {
        private readonly IEnumerable<IFileHandler> _handlers;
        private readonly IChatConfigurationService _config;
        private readonly IStore<FileAttachment> _store;
        public FileHandlerFactory(IServiceProvider services, IChatConfigurationService config, IStore<FileAttachment> store)
        {
            _handlers = services.GetServices<IFileHandler>();
            _config = config;
            _store = store;
        }

        public bool HasFileHandlers
        {
            get
            {
                return _handlers.Any();
            }
        }

        public bool CanHandleType(string contentType, string fileName)
        {
            foreach (var handler in _handlers)
                if(handler.HandlesType(contentType, fileName))
                    return true;
            return false;
        }

        public string[] GetSupportedMimeTypes()
        {
            var m = new List<string>();
            foreach (var handler in _handlers)
                m.AddRange(handler.GetSupportedMimeTypes());
            return m.ToArray();
        }

        public string? GetExtension(string mimeType)
        {
            mimeType = mimeType.ToLowerInvariant();
            foreach(var handler in _handlers)
            {
                var mapping = handler.GetExtensionMimeTypeMapping();
                foreach(var m in mapping)
                    if(m.Value ==  mimeType) return m.Key;
            }
            return null;
        }

        public async Task<KernelContent> GetKernelContent(string fileId, CancellationToken cancellationToken = default)
        {
            var file = await _store.FindAsync(x => x.Id == fileId, cancellationToken);
            if (file == null || !File.Exists(file.Path))
                return new TextContent("[FILE NOT FOUND]");
            try
            {
                foreach(var item in _handlers)
                    if(item.HandlesType(file.MimeType, file.Id))
                    {
                        var data = File.ReadAllBytes(file.Path);
                        var metadata = new Dictionary<string, object?>()
                        {
                            {"name", file.Id }
                        };
                        var content = await item.GetKernelContent(data, file.MimeType, file.Id, metadata);
                        return content ?? new TextContent($"[FILE TYPE NOT SUPPPORTED]");
                    }
                return new TextContent($"[FILE TYPE NOT SUPPPORTED]");
            }
            catch(Exception ex)
            {
                return new TextContent($"[ERROR message={ex.Message}]");
            }
        }

        public async Task<KernelContent?> SaveAsync(Stream data, string contentType, string name, CancellationToken cancellationToken = default)
        {
            if(!Directory.Exists(_config.Config.FileStoragePath))
                Directory.CreateDirectory(_config.Config.FileStoragePath);
            foreach (var item in _handlers)
            {
                if (item.HandlesType(contentType, name))
                {
                    var att = await SaveFileAttachment(contentType, name, cancellationToken);
                    using var ms = new MemoryStream();
                    await data.CopyToAsync(ms);
                    await File.WriteAllBytesAsync(att.Path, ms.ToArray(), cancellationToken);
                    return att.ToFileReference();
                }
            }

            return null;
        }

        public async Task<KernelContent?> SaveAsync(byte[] data, string contentType, string name, CancellationToken cancellationToken = default)
        {
            foreach (var item in _handlers)
            {
                if (item.HandlesType(contentType, name))
                {
                    var att = await SaveFileAttachment(contentType, name, cancellationToken);
                    await File.WriteAllBytesAsync(att.Path, data, cancellationToken);
                    return att.ToFileReference();
                }
            }

            return null;
        }

        private async Task<FileAttachment> SaveFileAttachment(string contentType, string name, CancellationToken cancellationToken = default)
        {
            var count = await _store.CountAsync(x => x.InternalName == name);
            var file_name = count == 0 ? name : $"{Path.GetFileNameWithoutExtension(name)}({count}){Path.GetExtension(name)}";
            var att = FileAttachment.From(file_name, contentType, Path.Combine(_config.Config.FileStoragePath, file_name), name);
            await _store.SaveAsync(att, cancellationToken);
            return att;
        }
#pragma warning disable SKEXP0110
        public async Task CancelAsync(KernelContent item, CancellationToken cancellationToken = default)
        {
            if (item is FileReferenceContent file)
            {
                var ext = await _store.FindAsync(x => x.Id == file.FileId, cancellationToken);
                if (ext == null) return;
                if (File.Exists(ext.Path))
                    File.Delete(ext.Path);
                await _store.DeleteAsync(ext);
            }
        }
#pragma warning restore SKEXP0110
    }
}
