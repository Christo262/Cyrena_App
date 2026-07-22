using Microsoft.SemanticKernel;
using System.Text.Json.Serialization;

namespace Cyrena.Models
{
    public sealed class FileAttachment : Entity
    {
        [JsonConstructor]
        internal FileAttachment()
        {
            Tools = new List<string>();
            Properties = new Dictionary<string, string?>();
        }
        public string MimeType { get; set; } = default!;
        public string Path { get; set; } = default!;
        public string InternalName { get; set; } = default!;
        public List<string> Tools { get; set; }
        public Dictionary<string, string?> Properties { get; set; }
        public string? this[string key]
        {
            get
            {
                if(Properties.ContainsKey(key))
                    return Properties[key];
                return null;
            }
            set
            {
                Properties[key] = value;
            }
        }

        public static FileAttachment From(string file_name, string content_type, string path, string original_name, params string[] tools)
        {
            if (!tools.Any())
                tools = ["Attachment_get"];
            return new FileAttachment()
            {
                Id = file_name,
                InternalName = original_name,
                Path = path,
                MimeType = content_type,
                Tools = tools.ToList(),
            };
        }
#pragma warning disable SKEXP0110
        public FileReferenceContent ToFileReference()
        {
            return new FileReferenceContent(Id)
            {
                MimeType = MimeType,
                Tools = Tools,
                Metadata = new Dictionary<string, object?>()
                {
                    {"name", Id }
                }
            };
        }
#pragma warning restore SKEXP0110
    }
}
