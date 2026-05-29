namespace Cyrena.Canvas.Models
{
    public class CanvasViewModel
    {
        public CanvasViewModel(string documentId, string? name, CanvasDocumentType documentType)
        {
            DocumentId = documentId;
            Name = name;
            DocumentType = documentType;
        }

        public CanvasViewModel(CanvasDocument doc)
        {
            Name = doc.Name;
            DocumentType = doc.DocumentType;
            DocumentId = doc.Id;
        }

        public string DocumentId { get; set; } = default!;
        public string? Name { get; set; }
        public CanvasDocumentType DocumentType { get; set; }
    }
}
