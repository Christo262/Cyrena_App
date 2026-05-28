using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Ollama.Web.Contracts;
using Cyrena.Ollama.Web.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Cyrena.Ollama.Web.Services
{
    internal class OllamaWebKernelFunctions
    {
        private readonly IOllamaWebSearchService _web;
        private readonly IChatMessageService _chat;
        public OllamaWebKernelFunctions(IOllamaWebSearchService web, IChatMessageService chat)
        {
            _web = web;
            _chat = chat;
        }

        [KernelFunction("search")]
        [Description("Performs a web search using Ollama Web.")]
        public async Task<ToolResult<SearchResults>> SearchAsync(
            [Description("The search query to look up.")] string query,
            [Description("The maximum number of results to return. Default is 5.")] int max_results = 5,
            CancellationToken cancellationToken = default)
        {
            await _chat.LogInfo("Searching Ollama Web...");
            return await _web.SearchAsync(query, max_results, cancellationToken);
        }

        [KernelFunction("fetch")]
        [Description("Fetches the full content of a specific URL using Ollama Web.")]
        public async Task<ToolResult<FetchResult>> FetchAsync(
            [Description("The full URL of the page to fetch.")] string url,
            CancellationToken cancellationToken = default)
        {
            await _chat.LogInfo("Fetchin with Ollama Web...");
            return await _web.FetchAsync(url, cancellationToken);
        }
    }
}
