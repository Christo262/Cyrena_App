using Cyrena.Models;

namespace Cyrena.Developer.Models
{
    /// <summary>
    /// Notes about the project the AI can make for itself
    /// </summary>
    public sealed class StickyNote : Entity
    {

        public StickyNote() { }
        public StickyNote(string? title, string? content)
        {
            Id = Guid.NewGuid().ToString();
            Title = title;
            Content = content;
        }
        public string? Title { get; set; }
        public string? Content { get; set; }
    }
}
