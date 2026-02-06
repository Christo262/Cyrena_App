using Cyrena.Models;

namespace Cyrena.Tavily.Models
{
    public class ExtractResponse : JsonStringObject
    {
        public ExtractResponse()
        {
            Results = new List<ExtractResult>();
            FailedResults = new List<ExtractFailure>();
        }
        public int StatusCode { get; set; }
        public string? Error { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("results")]
        public List<ExtractResult> Results { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("failed_results")]
        public List<ExtractFailure> FailedResults { get; set; }
    }

    public class ExtractResult : JsonStringObject
    {
        [System.Text.Json.Serialization.JsonPropertyName("url")]
        public string? Url { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("title")]
        public string? Title { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("raw_content")]
        public string? RawContent { get; set; }
    }

    public class ExtractFailure : JsonStringObject
    {
        [System.Text.Json.Serialization.JsonPropertyName("url")]
        public string? Url { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("error")]
        public string? Error { get; set; }
    }
}
