using Cyrena.Models;

namespace Cyrena.Runtime.Models
{
    public class Note : Entity
    {
        public string ProjectId { get; set; } = default!;
        public string Subject { get; set; } = default!;
        public string? Content { get; set; }
    }

    public record NoteRecord(string Id, string Subject, string? Content);
    public record NoteSubject(string Id, string Subject);
}
