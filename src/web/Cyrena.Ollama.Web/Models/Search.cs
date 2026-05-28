using System.Text.Json.Serialization;

namespace Cyrena.Ollama.Web.Models
{
    public class Search
    {
        [JsonPropertyName("query")]
        public string? Query { get; set; }
        [JsonPropertyName("max_results")]
        public int MaxResults { get; set; } = 5;
    }
}
