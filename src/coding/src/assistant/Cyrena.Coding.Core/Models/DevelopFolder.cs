using System.Text.Json.Serialization;

namespace Cyrena.Coding.Models
{
    public class DevelopFolder : FileTypeAllowanceDevelopItem
    {
        public List<DevelopFile> Files { get; set; } = [];
        public List<DevelopFolder> Folders { get; set; } = [];
        
        [JsonIgnore]
        internal bool IsVirtual { get; set; }
    }
}
