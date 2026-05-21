using Cyrena.Models;
using System.Text.Json.Serialization;

namespace Cyrena.Canvas.Models
{
    public class CanvasDocument : Entity
    {
        public CanvasDocument() { }

        [JsonIgnore]
        public string ConversationId { get; set; } = default!;
        public CanvasDocumentType DocumentType { get; set; }
        [JsonIgnore]
        public string? Content { get; set; }
        public string? Title { get; set; }
        [JsonIgnore]
        public string? Path { get; set; }
    }
}
