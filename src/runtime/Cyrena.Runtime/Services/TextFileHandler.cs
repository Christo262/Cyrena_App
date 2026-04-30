using Cyrena.Contracts;
using Cyrena.Models;
using Microsoft.SemanticKernel;

namespace Cyrena.Runtime.Services
{
    internal class TextFileHandler : IFileHandler
    {
        public async Task<AdditionalMessageContent?> GetMessageContent(Stream data, string contentType, string name)
        {
            if (!HandlesType(contentType, name))
                return null;
            using var ms = new MemoryStream();
            await data.CopyToAsync(ms);
            ms.Position = 0;

            using var reader = new StreamReader(ms);
            var textContent = await reader.ReadToEndAsync();
            var c = new TextContent($"--- File: {name} ---\n\n{textContent}");
            var content = new AdditionalMessageContent(name, c);
            return content;
        }

        public string[] GetSupportedMimeTypes()
        {
            return ["text/*", "application/json"];
        }

        public bool HandlesType(string contentType, string fileName)
        {
            if (contentType.StartsWith("text/"))
                return true;

            if (contentType == "application/json")
                return true;

            var extension = Path.GetExtension(fileName).ToLower();
            var textExtensions = new[]
            {
                ".txt", ".cs", ".js", ".ts", ".py", ".java", ".cpp", ".c",
                ".html", ".css", ".xml", ".json", ".md", ".yaml", ".yml",
                ".sh", ".bash", ".go", ".rs", ".rb", ".php", ".sql"
            };

            return textExtensions.Contains(extension);
        }
    }
}
