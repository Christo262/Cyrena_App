using System.Text.Json.Serialization;

namespace Cyrena.Extensa.Models
{
    public class ExtensionInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;
        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        [JsonPropertyName("version")]
        public Version Version { get; set; } = Version.Parse("1.0.0");
        [JsonPropertyName("entryAssemblyFile")]
        public string? EntryAssemblyFile { get; set; }
        [JsonPropertyName("dependencies")]
        public Dependency[] Dependencies { get; set; } = [];
    }
}
