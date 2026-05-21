using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.ComponentModel.DataAnnotations;

namespace Cyrena.Models
{
    public sealed class QueuedInput
    {
        public QueuedInput(ChatMessageContent message)
        {
            Id = Guid.NewGuid().ToString();
            Message = message;
        }

        public string Id { get; }
        public AuthorRole Role => Message.Role;
        public ChatMessageContent Message { get; set; }
    }
}
