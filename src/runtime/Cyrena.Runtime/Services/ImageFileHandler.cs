using Cyrena.Contracts;
using Cyrena.Models;
using Microsoft.SemanticKernel;

namespace Cyrena.Runtime.Services
{
    internal class ImageFileHandler : IFileHandler
    {
        public async Task<AdditionalMessageContent?> GetMessageContent(Stream data, string contentType, string name)
        {
            if (!contentType.StartsWith("image/"))
                return null;    
            using var ms = new MemoryStream();
            await data.CopyToAsync(ms);
            ms.Position = 0;
            var c = new ImageContent(ms.ToArray(), contentType);
            var content = new AdditionalMessageContent(name, c);
            return content;
        }

        public async Task<AdditionalMessageContent?> GetMessageContent(byte[] data, string contentType, string name)
        {
            if (!contentType.StartsWith("image/"))
                return null;
            var c = new ImageContent(data, contentType);
            var content = new AdditionalMessageContent(name, c);
            return content;
        }

        public string[] GetSupportedMimeTypes()
        {
            return ["image/*"];
        }

        public bool HandlesType(string contentType, string fileName)
        {
            return contentType.StartsWith("image/");
        }
    }
}
