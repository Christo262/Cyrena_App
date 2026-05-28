using System.Text.Json.Serialization;

namespace Cyrena.Ollama.Web.Models
{
    public class Fetch
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }
}
