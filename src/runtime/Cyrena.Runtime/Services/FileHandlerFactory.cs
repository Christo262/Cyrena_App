using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Options;
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
#pragma warning disable SKEXP0110
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

        public async Task<IEnumerable<FileAttachment>> ListAttachmentsAsync(CancellationToken cancellationToken = default)
        {
            if(!HasFileHandlers)
                return Enumerable.Empty<FileAttachment>();
            return await _store.FindManyAsync(x => true, ct:cancellationToken);
        }

        public async Task<byte[]> GetFileDataAsync(string id, CancellationToken cancellationToken = default)
        {
            if (!HasFileHandlers)
                throw new InvalidOperationException("No file handlers available");
            var att = await _store.FindAsync(x => x.Id == id, ct:cancellationToken);
            if (att == null)
                throw new NullReferenceException("Unable to find record of file");
            if (!File.Exists(att.Path))
                throw new FileNotFoundException("Unable to find file on disk");
            return File.ReadAllBytes(att.Path);
        }

        public async Task DeleteFileAttachmentAsync(string id, CancellationToken cancellationToken = default)
        {
            if (!HasFileHandlers)
                throw new InvalidOperationException("No file handlers available");
            var att = await _store.FindAsync(x => x.Id == id, ct: cancellationToken);
            if(att != null)
            {
                await _store.DeleteAsync(att, cancellationToken);
                if(File.Exists(att.Path))
                    File.Delete(att.Path);
            }
        }

        public async Task<FileAttachment> CreateAsync(string name, string contentType, byte[] content, CancellationToken cancellationToken = default)
        {
            var att = await SaveFileAttachment(contentType, name, cancellationToken);
            if (!Directory.Exists(_config.Config.FileStoragePath))
                Directory.CreateDirectory(Path.Combine(_config.Config.FileStoragePath));
            File.WriteAllBytes(att.Path, content);
            return att;
        }

        public async Task UpdateAsync(FileAttachment att, CancellationToken cancellationToken = default)
        {
            var ext = await _store.FindAsync(x => x.Id == att.Id, ct: cancellationToken);
            if (ext == null)
                throw new FileNotFoundException($"Unable to find file with id {att.Id}");
            ext.Properties = att.Properties;
            ext.Tools = att.Tools;
            await _store.SaveAsync(ext, cancellationToken);
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
                    var content = await item.GetKernelContent(ms.ToArray(), contentType, name);
                    if (content == null)
                        return att.ToFileReference();
                    var jn = new JoinedKernelContent([att.ToFileReference(),content], att.ToFileReference());
                    return jn;
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
                    var content = await item.GetKernelContent(data, contentType, name);
                    if (content == null)
                        return att.ToFileReference();
                    var jn = new JoinedKernelContent([att.ToFileReference(),content], att.ToFileReference());
                    return jn;
                }
            }

            return null;
        }

        private async Task<FileAttachment> SaveFileAttachment(string contentType, string name, CancellationToken cancellationToken = default)
        {
            var count = await _store.CountAsync(x => x.InternalName == name);
            var file_name = count == 0 ? name : $"{Path.GetFileNameWithoutExtension(name)}({count}){Path.GetExtension(name)}";
            if(!Directory.Exists(_config.Config.FileStoragePath))
                Directory.CreateDirectory(_config.Config.FileStoragePath);
            var att = FileAttachment.From(file_name, contentType, Path.Combine(_config.Config.FileStoragePath, file_name), name);
            await _store.SaveAsync(att, cancellationToken);
            return att;
        }


        public async Task CancelAsync(KernelContent item, CancellationToken cancellationToken = default)
        {
            if (item is FileReferenceContent file)
            {
                var ext = await _store.FindAsync(x => x.Id == file.FileId, cancellationToken);
                if (ext == null) return;
                if (File.Exists(ext.Path))
                    File.Delete(ext.Path);
                await _store.DeleteAsync(ext, cancellationToken);
            }

            if(item is ImageContent image)
            {
                var id = image.Metadata?.ContainsKey("name") == true ? image.Metadata["name"]?.ToString() : null;
                if (string.IsNullOrEmpty(id)) return;
                var ext = await _store.FindAsync(x => x.Id == id, cancellationToken);
                if (ext == null) return;
                if (File.Exists(ext.Path))
                    File.Delete(ext.Path);
                await _store.DeleteAsync(ext, cancellationToken);
            }

            if (item is JoinedKernelContent joined && joined.SaveAs is FileReferenceContent frc)
            {
                var ext = await _store.FindAsync(x => x.Id == frc.FileId, cancellationToken);
                if (ext == null) return;
                if (File.Exists(ext.Path))
                    File.Delete(ext.Path);
                await _store.DeleteAsync(ext, cancellationToken);
            }
        }
#pragma warning restore SKEXP0110

        public async Task<FileAttachment?> GetAttachmentAsync(string id, CancellationToken cancellationToken = default)
        {
            var item = await _store.FindAsync(x => x.Id ==  id, cancellationToken);
            return item;
        }
    }
}
