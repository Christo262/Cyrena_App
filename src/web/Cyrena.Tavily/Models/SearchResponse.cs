using Cyrena.Models;

namespace Cyrena.Tavily.Models
{
    public class SearchResponse : JsonStringObject
    {
        public SearchResponse()
        {
            Images = new List<Image>();
            Results = new List<Result>();
        }

        public int StatusCode { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("query")]
        public string? Query { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("answer")]
        public string? Answer { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("images")]
        public List<Image> Images { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("results")]
        public List<Result> Results { get; set; }
    }

    public class Image : JsonStringObject
    {
        [System.Text.Json.Serialization.JsonPropertyName("url")]
        public string? Url { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    public class Result : JsonStringObject
    {
        [System.Text.Json.Serialization.JsonPropertyName("url")]
        public string? Url { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("title")]
        public string? Title { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("score")]
        public double Score { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("published_date")]
        public string? PublishedDate { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("content")]
        public string? Content { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("raw_content")]
        public string? RawContent { get; set; }
    }
}
