using Microsoft.SemanticKernel.ChatCompletion;
using System.ComponentModel.DataAnnotations;

namespace Cyrena.Models
{
    public sealed class QueuedInput
    {
        private string _content;

        public QueuedInput(AuthorRole role, string? content, AdditionalMessageContent[]? items)
        {
            Id = Guid.NewGuid().ToString();
            Role = role;
            _content = content ?? string.Empty;
            Items = new List<AdditionalMessageContent>(items ?? Array.Empty<AdditionalMessageContent>());
        }

        public string Id { get; }
        public AuthorRole Role { get; }

        [Required]
        public string Content
        {
            get => _content;
            set => _content = value ?? string.Empty;
        }

        public List<AdditionalMessageContent> Items { get; set; }
    }
}
