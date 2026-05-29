using Cyrena.Models;
using System.Text.Json.Serialization;

namespace Cyrena.Canvas.Models
{
    public class CanvasDocument : Entity
    {
        public CanvasDocument() { }

        public CanvasDocumentType DocumentType { get; set; }
        [JsonIgnore]
        public string? Content { get; set; }
        public string? Name { get; set; }
        [JsonIgnore]
        public string? Path { get; set; }
        public string? Language { get; set; }
    }
}
