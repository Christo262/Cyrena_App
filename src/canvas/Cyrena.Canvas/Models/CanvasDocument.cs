using Cyrena.Models;

namespace Cyrena.Canvas.Models
{
    public class CanvasDocument : Entity
    {
        public string ConversationId { get; set; } = default!;
        public CanvasDocumentType DocumentType { get; set; }
        public string? Content { get; set; }
        public string? Title { get; set; }
    }
}
