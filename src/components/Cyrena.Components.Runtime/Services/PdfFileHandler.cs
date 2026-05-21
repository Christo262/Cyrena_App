using Cyrena.Contracts;
using Cyrena.Models;
using Microsoft.SemanticKernel;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Cyrena.Services
{
    internal class PdfFileHandler : IFileHandler
    {
        public async Task<KernelContent?> GetKernelContent(byte[] data, string contentType, string name, IReadOnlyDictionary<string, object?>? metadata = null)
        {
            if (!HandlesType(contentType, name))
                return null;
            using var ms = new MemoryStream(data);
            ms.Position = 0;
            var pdfText = ExtractTextFromPdf(ms, name);
            var c = new TextContent(pdfText, metadata:metadata) { MimeType = contentType };
            return c;
        }

        public string[] GetSupportedMimeTypes()
        {
            return ["application/pdf"];
        }

        public bool HandlesType(string contentType, string fileName)
        {
            return contentType == "application/pdf" ||
                   fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
        }

        public Dictionary<string, string> GetExtensionMimeTypeMapping()
        {
            return new() { { ".pdf", "application/pdf" } };
        }

        private string ExtractTextFromPdf(Stream pdfStream, string fileName)
        {
            try
            {
                using var document = PdfDocument.Open(pdfStream);
                var textBuilder = new StringBuilder();

                foreach (Page page in document.GetPages())
                {
                    textBuilder.AppendLine($"[Page {page.Number}]");
                    textBuilder.AppendLine(ContentOrderTextExtractor.GetText(page));
                    textBuilder.AppendLine();
                }

                return textBuilder.ToString();
            }
            catch (Exception ex)
            {
                return $"--- PDF: {fileName} ---\nError extracting text: {ex.Message}";
            }
        }
    }
}
