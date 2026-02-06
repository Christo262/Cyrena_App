using Cyrena.Models;

namespace Cyrena.Tavily.Models
{
    public class ExtractRequest : JsonStringObject
    {
        public ExtractRequest()
        {
            Urls = new List<string>();
        }

        [System.Text.Json.Serialization.JsonPropertyName("urls")]
        public List<string> Urls { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("query")]
        public string? Query { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("chunks_per_response")]
        public int ChunksPerResponse { get; set; } = 3;
        [System.Text.Json.Serialization.JsonPropertyName("extract_depth")]
        public string ExtractDepth { get; set; } = "basic";
        [System.Text.Json.Serialization.JsonPropertyName("format")]
        public string Format { get; set; } = "markdown";
    }
}
