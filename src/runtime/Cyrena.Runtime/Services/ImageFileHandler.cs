using Cyrena.Contracts;
using Cyrena.Models;
using Microsoft.SemanticKernel;

namespace Cyrena.Runtime.Services
{
    internal class ImageFileHandler : IFileHandler
    {
        public async Task<KernelContent?> GetKernelContent(byte[] data, string contentType, string name, IReadOnlyDictionary<string, object?>? metadata = null)
        {
            if (!contentType.StartsWith("image/"))
                return null;
            var c = new ImageContent(data, contentType) { MimeType = contentType, Metadata = metadata };
            return c;
        }

        public string[] GetSupportedMimeTypes()
        {
            return ["image/*"];
        }

        public Dictionary<string, string> GetExtensionMimeTypeMapping()
        {
            return new()
            {
                {".png", "image/png" },
                {".jpg", "image/jpeg" },
                {".jpeg", "image/jpeg" },
                {".gif", "image/gif" },
                {".webp", "image/webp" },
                {".ico", "image/x-icon" }
            };
        }

        public bool HandlesType(string contentType, string fileName)
        {
            return contentType.StartsWith("image/");
        }
    }
}
