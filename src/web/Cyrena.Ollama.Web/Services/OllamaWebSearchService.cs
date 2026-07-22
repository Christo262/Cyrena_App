using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Ollama.Web.Contracts;
using Cyrena.Ollama.Web.Models;
using Cyrena.Ollama.Web.Options;
using System.Net.Http.Json;

namespace Cyrena.Ollama.Web.Services
{
    internal class OllamaWebSearchService : IOllamaWebSearchService
    {
        private readonly ISettingsService _settings;
        private readonly HttpClient _http;
        public OllamaWebSearchService(ISettingsService settings)
        {
            _settings = settings;
            if (OperatingSystem.IsAndroid())
            {
                _http = new HttpClient(new OllamaAndroidHandler(new HttpClientHandler()))
                {
                    BaseAddress = new("https://ollama.com")
                };
            }
            else
            {
                _http = new HttpClient()
                {
                    BaseAddress = new("https://ollama.com")
                };
            }
        }

        public async Task<ToolResult<SearchResults>> SearchAsync(string? query, int max_results = 5, CancellationToken cancellationToken = default)
        {
            if(!CheckAvailable(out var key))
                return SearchUnavailable();
            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", key);
            try
            {
                var model = new Search() { MaxResults = max_results , Query = query};
                using var response = await _http.PostAsJsonAsync("/api/web_search", model, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return new ToolResult<SearchResults>(false, $"Invalid HTTP response code: {response.StatusCode}");
                }
                var result = await response.Content.ReadFromJsonAsync<SearchResults>();
                if (result == null)
                    return new ToolResult<SearchResults>(false, "Unable to deserialize response");
                return new ToolResult<SearchResults>(result);
            }
            catch (Exception ex)
            {
                return new ToolResult<SearchResults>(false, ex.Message);
            }
        }

        public async Task<ToolResult<FetchResult>> FetchAsync(string? url, CancellationToken cancellationToken = default)
        {
            if (!CheckAvailable(out var key))
                return FetchUnavailable();
            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", key);
            try
            {
                var model = new Fetch() { Url = url };
                using var response = await _http.PostAsJsonAsync("/api/web_fetch", model, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return new ToolResult<FetchResult>(false, $"Invalid HTTP response code: {response.StatusCode}");
                }
                var result = await response.Content.ReadFromJsonAsync<FetchResult>();
                if (result == null)
                    return new ToolResult<FetchResult>(false, "Unable to deserialize response");
                return new ToolResult<FetchResult>(result);
            }
            catch (Exception ex)
            {
                return new ToolResult<FetchResult>(false, ex.Message);
            }
        }

        private bool CheckAvailable(out string? key)
        {
            var options = _settings.Read<OllamaWebOptions>(OllamaWebOptions.Key);
            key = options?.APIKey;
            if(options == null || !options.Enabled || string.IsNullOrEmpty(options.APIKey))
                return false;
            return true;
        }

        private ToolResult<FetchResult> FetchUnavailable()
        {
            var options = _settings.Read<OllamaWebOptions>(OllamaWebOptions.Key);
            if (options == null)
                return new ToolResult<FetchResult>(false, "Web search configuration incomplete");
            if (!options.Enabled)
                return new ToolResult<FetchResult>(false, "Web search has been disabled by user");
            if (string.IsNullOrEmpty(options.APIKey))
                return new ToolResult<FetchResult>(false, "API Key not configured");
            return new ToolResult<FetchResult>(false, "Unavailable");
        }

        private ToolResult<SearchResults> SearchUnavailable()
        {
            var options = _settings.Read<OllamaWebOptions>(OllamaWebOptions.Key);
            if (options == null)
                return new ToolResult<SearchResults>(false, "Web search configuration incomplete");
            if (!options.Enabled)
                return new ToolResult<SearchResults>(false, "Web search has been disabled by user");
            if (string.IsNullOrEmpty(options.APIKey))
                return new ToolResult<SearchResults>(false, "API Key not configured");
            return new ToolResult<SearchResults>(false, "Unavailable");
        }
    }
}
