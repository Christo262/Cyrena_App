using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;
using System.Text.Json.Serialization;

namespace Cyrena.Models
{
    /// <summary>
    /// Used to persist chat history
    /// </summary>
    public sealed class ChatMessageContentEntity : Entity
    {
        public ChatMessageContentEntity()
        {
            Id = Ulid.NewUlid().ToString();
            Date = DateTime.Now;
            Items = new List<KernelContentEntity>();
        }

        public ChatMessageContentEntity(ChatMessageContent content, Ulid? iterationId = null)
        {
            Id = Ulid.NewUlid().ToString();
            IterationId = iterationId;
            Date = DateTime.Now;
            Items = content.Items.Select(x => new KernelContentEntity(x)).ToList();
            MimeType = content.MimeType;
            Role = content.Role.Label;
        }

        public Ulid? IterationId { get; set; }
        public DateTime Date { get; set; }
        public string? Role { get; set; }

        public List<KernelContentEntity> Items { get; set; }

        /// <summary>
        /// The model ID used to generate the content.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ModelId { get; set; }

        /// <summary>
        /// The metadata associated with the content.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IReadOnlyDictionary<string, object?>? Metadata { get; set; }

        /// <summary>
        /// MIME type of the content.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? MimeType { get; set; }

        public ChatMessageContent AsChatMessage()
        {
            var items = new ChatMessageContentItemCollection();
            foreach (var item in this.Items)
                if(item.Item != null)
                    items.Add(item.Item);
            return new ChatMessageContent(new AuthorRole(Role ?? "user"), items, ModelId, null, Encoding.UTF8, Metadata);
        }
    }

    public sealed class KernelContentEntity : Entity
    {
        public KernelContentEntity()
        {
            Id = Ulid.NewUlid().ToString();
        }

        public KernelContentEntity(KernelContent item)
        {
            Id = Ulid.NewUlid().ToString();
            if(item.Metadata?.ContainsKey("save-as") == true)
            {
                var target = item.Metadata["save-as"];
                if(target is KernelContent ctl)
                    Item = ctl;
            }
            else
            {
                Item = item;
            }
        }

        public KernelContent? Item { get; set; }

        public bool IsContentType<TKernelContent>()
            where TKernelContent : KernelContent
        {
            if(Item == null) return false;
            return Item.GetType() == typeof(TKernelContent);
        }
    }
}
