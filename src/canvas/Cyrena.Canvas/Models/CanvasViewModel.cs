namespace Cyrena.Canvas.Models
{
    public class CanvasViewModel
    {
        public CanvasViewModel(string documentId, string? title, CanvasDocumentType documentType)
        {
            DocumentId = documentId;
            Title = title;
            DocumentType = documentType;
        }

        public CanvasViewModel(CanvasDocument doc)
        {
            Title = doc.Title;
            DocumentType = doc.DocumentType;
            DocumentId = doc.Id;
        }

        public string DocumentId { get; set; } = default!;
        public string? Title { get; set; }
        public CanvasDocumentType DocumentType { get; set; }
    }
}
