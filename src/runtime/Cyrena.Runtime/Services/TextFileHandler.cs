using Cyrena.Contracts;
using Cyrena.Models;
using Microsoft.SemanticKernel;

namespace Cyrena.Runtime.Services
{
    internal class TextFileHandler : IFileHandler
    {
        private static Dictionary<string, string> _mapping = new()
        {
            {".txt", "text/plain" },
            {".json", "application/json" },
            {".js", "text/javascript" },
            {".csv", "text/csv" },
            {".xml", "text/xml" },
            {".css", "text/css" },
            {".html", "text/html" },
            {".md", "text/markdown" },
            {".ics", "text/calendar" },
            {".vcf", "text/vcard" },
            {".tsv", "text/tab-separated-values" }
        };
        private static string[] _supportedExtensions = [ ".cs", ".ts", ".py", ".java", ".cpp", ".c", ".yaml", ".yml",
                                                        ".sh", ".bash", ".go", ".rs", ".rb", ".php", ".sql"];
        public async Task<KernelContent?> GetKernelContent(byte[] data, string contentType, string name, IReadOnlyDictionary<string, object?>? metadata = null)
        {
            if (!HandlesType(contentType, name))
                return null;
            using var ms = new MemoryStream(data);
            ms.Position = 0;
            using var reader = new StreamReader(ms);
            var textContent = await reader.ReadToEndAsync();
            var c = new TextContent(textContent, metadata: metadata) { MimeType = contentType };
            return c;
        }

        public string[] GetSupportedMimeTypes()
        {
            return ["text/*", "application/json"];
        }

        public Dictionary<string, string> GetExtensionMimeTypeMapping()
        {
            var model = new Dictionary<string, string>(_mapping);
            foreach (var item in _supportedExtensions)
                model.Add(item, "text/plain");
            return model;
        }

        public bool HandlesType(string contentType, string fileName)
        {
            if (contentType.StartsWith("text/"))
                return true;

            if (contentType == "application/json" || contentType == "application/javascript" || contentType == "application/xml")
                return true;

            var extension = Path.GetExtension(fileName).ToLower();

            return _supportedExtensions.Contains(extension) || _mapping.ContainsKey(extension);
        }
    }
}
