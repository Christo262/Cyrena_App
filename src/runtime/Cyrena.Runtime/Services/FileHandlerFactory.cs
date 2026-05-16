using Cyrena.Contracts;
using Cyrena.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.Runtime.Services
{
    internal class FileHandlerFactory : IFileHandlerFactory
    {
        private readonly IEnumerable<IFileHandler> _handlers;
        public FileHandlerFactory(IServiceProvider services)
        {
            _handlers = services.GetServices<IFileHandler>();
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

        public Task<AdditionalMessageContent?> GetMessageContent(Stream data, string contentType, string name)
        {
            foreach(var handler in _handlers)
                if(handler.HandlesType(contentType, name))
                    return handler.GetMessageContent(data, contentType, name);
            return Task.FromResult<AdditionalMessageContent?>(null);
        }

        public Task<AdditionalMessageContent?> GetMessageContent(byte[] data, string contentType, string name)
        {
            foreach (var handler in _handlers)
                if (handler.HandlesType(contentType, name))
                    return handler.GetMessageContent(data, contentType, name);
            return Task.FromResult<AdditionalMessageContent?>(null);
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

        public Task<KernelContent?> GetKernelContent(Stream data, string contentType, string name)
        {
            foreach (var handler in _handlers)
                if (handler.HandlesType(contentType, name))
                    return handler.GetKernelContent(data, contentType, name);
            return Task.FromResult<KernelContent?>(null);
        }

        public Task<KernelContent?> GetKernelContent(byte[] data, string contentType, string name)
        {
            foreach (var handler in _handlers)
                if (handler.HandlesType(contentType, name))
                    return handler.GetKernelContent(data, contentType, name);
            return Task.FromResult<KernelContent?>(null);
        }
    }
}
