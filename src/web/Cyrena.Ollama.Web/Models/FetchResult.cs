using System.Text.Json.Serialization;

namespace Cyrena.Ollama.Web.Models
{
    public class FetchResult 
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }
        [JsonPropertyName("content")]
        public string? Content { get; set; }
        [JsonPropertyName("links")]
        public string[] Links { get; set; } = [];
    }
}
