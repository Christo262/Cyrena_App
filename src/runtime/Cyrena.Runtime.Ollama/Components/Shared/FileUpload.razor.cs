using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Runtime.Ollama.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Microsoft.SemanticKernel;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Cyrena.Runtime.Ollama.Components.Shared
{
    public partial class FileUpload
    {
        [Inject] private IJSRuntime _js { get; set; } = default!;
        private IIterationService _its { get; set; } = default!;
        private OllamaConnectionInfo _info = default!;
        private string _accepts { get; set; } = default!;
        protected override void OnInitialized()
        {
            _its = Kernel.Services.GetRequiredService<IIterationService>();
            _info = Kernel.Services.GetRequiredService<OllamaConnectionInfo>();
            if (_info.SupportsImage)
                _accepts += $"image/*{(_info.SupportsFile ? "," : "")}";
            if (_info.SupportsFile)
                _accepts += "text/*,application/pdf,application/json";
        }

        private async Task TriggerFileUpload()
        {
            await _js.InvokeVoidAsync("triggerClick", _fileInput.Element);
        }

        private InputFile _fileInput = null!;

        private async Task HandleFilesSelected(InputFileChangeEventArgs e)
        {
            if (_its.Inferring) return;
            var files = e.GetMultipleFiles(maximumFileCount: 10);
            List<AdditionalMessageContent> models = new List<AdditionalMessageContent>();

            foreach (var file in files)
            {
                using var stream = file.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024); // 50MB limit
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                ms.Position = 0; // Important: reset position after copying

                if (file.ContentType.Contains("image/"))
                {
                    var c = new ImageContent(ms.ToArray(), file.ContentType);
                    models.Add(new AdditionalMessageContent(file.Name, c));
                }
                else if (IsPdfFile(file.ContentType, file.Name))
                {
                    var pdfText = ExtractTextFromPdf(ms, file.Name);
                    var c = new TextContent(pdfText);
                    models.Add(new(file.Name, c));
                }
                else if (IsTextFile(file.ContentType, file.Name))
                {
                    using var reader = new StreamReader(ms);
                    var textContent = await reader.ReadToEndAsync();
                    var c = new TextContent($"--- File: {file.Name} ---\n\n{textContent}");
                    models.Add(new(file.Name, c));
                }
            }

            if (models.Count > 0)
                await OnItemsAdded.InvokeAsync(models.ToArray());

            StateHasChanged();
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

        private bool IsPdfFile(string contentType, string fileName)
        {
            return contentType == "application/pdf" ||
                   fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsTextFile(string contentType, string fileName)
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
