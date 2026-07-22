using Cyrena.Models;
using Cyrena.Ollama.Web.Models;

namespace Cyrena.Ollama.Web.Contracts
{
    public interface IOllamaWebSearchService
    {
        Task<ToolResult<SearchResults>> SearchAsync(string? query, int max_results = 5, CancellationToken cancellationToken = default);
        Task<ToolResult<FetchResult>> FetchAsync(string? url, CancellationToken cancellationToken = default);
    }
}
