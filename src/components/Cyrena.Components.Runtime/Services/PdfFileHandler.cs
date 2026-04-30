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
        public async Task<AdditionalMessageContent?> GetMessageContent(Stream data, string contentType, string name)
        {
            if (!HandlesType(contentType, name))
                return null;
            using var ms = new MemoryStream();
            await data.CopyToAsync(ms);
            ms.Position = 0;
            var pdfText = ExtractTextFromPdf(ms, name);
            var c = new TextContent(pdfText);
            var content = new AdditionalMessageContent(name, c);
            return content;
        }

        public async Task<AdditionalMessageContent?> GetMessageContent(byte[] data, string contentType, string name)
        {
            if (!HandlesType(contentType, name))
                return null;
            using var ms = new MemoryStream(data);
            ms.Position = 0;
            var pdfText = ExtractTextFromPdf(ms, name);
            var c = new TextContent(pdfText);
            var content = new AdditionalMessageContent(name, c);
            return content;
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

        private string ExtractTextFromPdf(Stream pdfStream, string fileName)
        {
            try
            {
                using var document = PdfDocument.Open(pdfStream);
                var textBuilder = new StringBuilder();

                textBuilder.AppendLine($"--- PDF: {fileName} ---");
                textBuilder.AppendLine();

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
