using Microsoft.SemanticKernel;
using Cyrena.Contracts;
using Cyrena.Tavily.Models;
using Cyrena.Tavily.Options;
using System.ComponentModel;
using System.Net.Http.Json;

namespace Cyrena.Tavily.Plugins
{
    internal class TavilyPlugin
    {
        private readonly HttpClient _http;
        private readonly TavilyOptions _options;
        private readonly IDeveloperContext _context;
        public TavilyPlugin(TavilyOptions options, IDeveloperContext context)
        {
            _options = options;
            _context = context;
            _http = new HttpClient()
            {
                BaseAddress = new Uri("https://api.tavily.com")
            };
            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiKey);
        }

        [KernelFunction]
        [Description("Performs a web search.")]
        public async Task<SearchResponse> WebSearchAsync(
            [Description("The query to search for.")]string query, 
            [Description("The topic in which to search, only valid options are 'general', 'news' or 'finance'.")]string topic = "general", 
            [Description("Depth of search to perform, only valid values are 'basic' or 'advanced'")]string search_depth = "basic",
            [Description("Number of results to include, default is 5.")]int max_results = 5,
            [Description("If images should be included in the search results. Default false.")]bool include_images = false,
            [Description("If image descriptions should be included in the search results. Default false.")] bool include_image_descriptions = false,
            [Description("If raw content should be included in the results. Valid options are 'False', 'text' or 'markdown'.")]string include_raw_content = "False")
        {
            var request = new SearchRequest()
            {
                Query = query,
                Topic = topic,
                SearchDepth = search_depth,
                IncludeImages = include_images,
                IncludeImageDescriptions = include_image_descriptions,
                IncludeRawContent = include_raw_content,
                MaxResults = max_results
            };
            try
            {
                _context.LogInfo($"Searching {query}");
                using var response = await _http.PostAsJsonAsync("/search", request);
                var model = await response.Content.ReadFromJsonAsync<SearchResponse>();
                var content = await response.Content.ReadAsStringAsync();
                if(model != null) 
                    model.StatusCode = (int)response.StatusCode;
                return model ?? new SearchResponse() { StatusCode = 501, Answer = "Unable to deserialize response", Query = query };
            }catch (Exception ex)
            {
                return new SearchResponse() { StatusCode = 500, Answer = ex.Message, Query = query };
            }
        }

        [KernelFunction]
        [Description("Extracts content from the provided URLs in markdown format")]
        public async Task<ExtractResponse> WebExtractAsync(
            [Description("An array of URLs to extract content from. Required.")]string[] urls, 
            [Description("Extract chunks relevant to the intent of the query. Optional.")]string? query = null)
        {
            var request = new ExtractRequest()
            {
                Urls = urls.ToList(),
                Query = query,
            };
            try
            {
                _context.LogInfo($"Extracting web results");
                using var response = await _http.PostAsJsonAsync("/extract", request);
                var model = await response.Content.ReadFromJsonAsync<ExtractResponse>();
                if(model != null)
                    model.StatusCode= (int)response.StatusCode;
                return model ?? new ExtractResponse() { StatusCode = 500, Error = "Unable to deserialize response" };
            }
            catch (Exception ex)
            {
                return new ExtractResponse() { StatusCode = 500, Error = ex.Message };
            }
        }
    }
}
