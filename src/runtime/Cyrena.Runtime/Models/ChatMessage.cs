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
            Date = DateTime.Now;
            AdditionalFiles = new List<string>();
        }

        public ChatMessage(string conversationId, AuthorRole role, string? content)
        {
            ConversationId = conversationId;
            Date = DateTime.Now;
            Label = role.Label;
            Content = content;
            AdditionalFiles = new List<string>();
        }

        public ChatMessage(ChatMessageContent content, string conversationId, AdditionalMessageContent[]? items = null)
        {
            ConversationId = conversationId;
            Date = DateTime.Now;
            Label = content.Role.Label;
            Content = content.Content;
            AdditionalFiles = new List<string>();
            if (items != null)
                AdditionalFiles.AddRange(items.Select(x => x.Name));
        }

        public string ConversationId { get; set; } = default!;
        public DateTime Date { get; set; }
        public string Label { get; set; } = default!;
        public string? Content { get; set; }

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
