using Cyrena.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Cyrena.Runtime.Models
{
    /// <summary>
    /// Just to save messages
    /// </summary>
    public class ChatMessage : Entity
    {
        public ChatMessage()
        {
            Id = Ulid.NewUlid().ToString();
            Date = DateTime.Now;
            AdditionalFiles = new List<string>();
        }

        public ChatMessage(string conversationId, AuthorRole role, string? content)
        {
            Id = Ulid.NewUlid().ToString();
            ConversationId = conversationId;
            Date = DateTime.Now;
            Label = role.Label;
            Content = content;
            AdditionalFiles = new List<string>();
        }

        public ChatMessage(ChatMessageContent content, string conversationId, string? iterationId = null, AdditionalMessageContent[]? items = null)
        {
            Id = Ulid.NewUlid().ToString();
            ConversationId = conversationId;
            Date = DateTime.Now;
            Label = content.Role.Label;
            Content = content.Content;
            AdditionalFiles = new List<string>();
            IterationId = iterationId;
            if (items != null)
                AdditionalFiles.AddRange(items.Select(x => x.Name));
        }

        public string ConversationId { get; set; } = default!;
        public string? IterationId { get; set; }
        public DateTime Date { get; set; }
        public string Label { get; set; } = default!;
        public string? Content { get; set; }
        /// <summary>
        /// Only applicable to <see cref="Cyrena.Options.ChatOptions.User"/> and <see cref="Cyrena.Options.ChatOptions.Assistant"/>.
        /// Allows overriding the Display logic to exclude a message from display history. Default false.
        /// </summary>
        public bool NoDisplay { get; set; } = false;

        public List<string> AdditionalFiles { get; set; }

        public ChatMessageContent ToDisplayMessageContent()
        {
            var model = new ChatMessageContent(new AuthorRole(Label), Content);
            if(AdditionalFiles.Count > 0)
                AdditionalFiles.ForEach(e => model.Items.Add(new InfoMessageContentItem(e)));
            return model;
        }
    }
}
