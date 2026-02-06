using Cyrena.Models;

namespace Cyrena.Tavily.Models
{
    public class SearchRequest : JsonStringObject
    {
        [System.Text.Json.Serialization.JsonPropertyName("query")]
        public string? Query { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("topic")]
        public string? Topic { get; set; } //general, news, finance
        [System.Text.Json.Serialization.JsonPropertyName("search_depth")]
        public string? SearchDepth { get; set; } //basic, advanced
        [System.Text.Json.Serialization.JsonPropertyName("max_results")]
        public int MaxResults { get; set; } = 5;
        [System.Text.Json.Serialization.JsonPropertyName("include_images")]
        public bool IncludeImages { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("include_image_descriptions")]
        public bool IncludeImageDescriptions { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("include_raw_content")]
        public string? IncludeRawContent { get; set; } //none, text, markdown
    }
}
