using System.Text.Json.Serialization;

namespace Cyrena.Ollama.Web.Models
{
    public class SearchResults
    {
        public SearchResults()
        {
            Results = new List<SearchResult>();
        }

        [JsonPropertyName("results")]
        public List<SearchResult> Results { get; set; }
    }

    public class SearchResult
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }
        [JsonPropertyName("url")]
        public string? Url { get; set; }
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}
