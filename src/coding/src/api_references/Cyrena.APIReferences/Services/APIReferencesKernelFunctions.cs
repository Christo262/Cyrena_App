using Cyrena.Contracts;
using Cyrena.APIReferences.Models;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Persistence.Contracts;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text;

namespace Cyrena.APIReferences.Services
{
    internal class APIReferencesKernelFunctions
    {
        private readonly IStore<ApiReference> _store;
        private readonly IChatMessageService _context;
        public APIReferencesKernelFunctions(IStore<ApiReference> store, IChatMessageService chat)
        {
            _store = store;
            _context = chat;
        }

        [KernelFunction("search")]
        [Description("Search API References for authoritative technical documentation about this project. Use this before implementing features to understand APIs, architecture rules, integration contracts, and established behavior.")]
        public async Task<ApiReferenceSearchCollection> Search(
            [Description("Keywords describing what API reference you are looking for (interfaces, services, architecture, styling, integration, etc.).")] string[] keywords,
            [Description("Maximum number of results to return. Default 10.")] int maxResults = 10)
        {
            await _context.LogInfo("Searching API References...");
            var normalized = keywords
                .Select(Normalize)
                .Distinct()
                .ToArray();

            var results = new List<ApiReferenceSearch>();
            var articles = await _store.FindManyAsync(x => true);

            foreach (var a in articles)
            {
                int score = 0;

                var title = Normalize(a.Title ?? "");
                var summary = Normalize(a.Summary ?? "");
                var content = Normalize(a.Content ?? "");

                var articleKeywords = a.Keywords
                    .Select(Normalize)
                    .ToHashSet();

                foreach (var k in normalized)
                {
                    if (articleKeywords.Contains(k))
                        score += 5;

                    if (title.Contains(k))
                        score += 10;

                    if (summary.Contains(k))
                        score += 3;

                    if (content.Contains(k))
                        score += 1;
                }

                if (score > 0)
                {
                    results.Add(new ApiReferenceSearch(a.Id, a.Title, a.Summary)
                    {
                        Score = score
                    });
                }
            }

            var re = results
                .OrderByDescending(r => r.Score)
                .Take(maxResults);
            return new ApiReferenceSearchCollection(re);
        }

        [KernelFunction("all")]
        [Description("List all API References available.")]
        public async Task<ApiReferenceSummaryCollection> ListAll()
        {
            var refs = await _store.FindManyAsync(x => true);
            var res = refs.Select(x => new ApiReferenceSummary(x.Id, x.Title, x.Summary));
            return new ApiReferenceSummaryCollection(res);
        }

        [KernelFunction("read")]
        [Description("Read a API Reference document. These documents contain grounded technical information about real project code and represent authoritative implementation knowledge.")]
        public async Task<ToolResult<ApiReference>> Read(
            [Description("The id of the reference document to read.")] string id)
        {
            var article = await _store.FindAsync(x => x.Id == id);
            if (article == null)
                return new ToolResult<ApiReference>(false, $"API Reference with id {id} not found");
            await _context.LogInfo($"Reading API Reference {article.Title}");
            return new ToolResult<ApiReference>(article);
        }

        [KernelFunction("write")]
        [Description(@"Creates or updates an API Reference document.

            API References are authoritative technical documentation grounded in actual project source code.

            Use this when creating a new reference or revising an existing reference after implementation changes.

            Rules:
            1. Read all relevant source files before writing API references about code.
            2. Base the reference only on real implementation.
            3. Never write generic, imagined, or hypothetical API behavior.
            4. Capture real method signatures, contracts, architecture rules, and usage patterns.
            5. If updating an existing reference, preserve accurate existing information unless the implementation changed.
            6. This document becomes authoritative project knowledge.")]
        public async Task<ToolResult<ApiReferenceSummary>> Write(
            [Description("The id of the API reference. Use an existing id to update. Use a stable new id to create.")]
    string id,

            [Description("Title of the API reference document. Mandatory.")]
    string title,

            [Description("Keywords used to search for this reference in the future. Mandatory.")]
    string[] keywords,

            [Description("Brief summary of what the reference contains. Mandatory.")]
    string summary,

            [Description("Grounded technical content in plaintext or markdown. Do not include Title, Summary or Keywords here. Mandatory.")]
    string content,

            [Description("Optional external or source link related to this reference.")]
    string? link = null,

            [Description("If true, creates the API reference when it does not already exist. If false, fails when the id is not found.")]
    bool createIfMissing = true)
        {
            if (string.IsNullOrWhiteSpace(id))
                return new ToolResult<ApiReferenceSummary>(false, "API reference id is required.");

            if (string.IsNullOrWhiteSpace(title))
                return new ToolResult<ApiReferenceSummary>(false, "Title is required.");

            if (keywords == null || keywords.Length == 0)
                return new ToolResult<ApiReferenceSummary>(false, "At least one keyword is required.");

            if (string.IsNullOrWhiteSpace(summary))
                return new ToolResult<ApiReferenceSummary>(false, "Summary is required.");

            if (string.IsNullOrWhiteSpace(content))
                return new ToolResult<ApiReferenceSummary>(false, "Content is required.");

            var article = await _store.FindAsync(x => x.Id == id);

            if (article == null)
            {
                if (!createIfMissing)
                    return new ToolResult<ApiReferenceSummary>(false, $"API Reference with id {id} not found.");

                article = new ApiReference
                {
                    Id = id,
                    Title = title,
                    Keywords = keywords,
                    Summary = summary,
                    Content = content,
                    Link = link
                };

                await _context.LogInfo($"Creating API Reference: {title}");
                await _store.SaveAsync(article);
            }
            else
            {
                await _context.LogInfo($"Updating API Reference: {article.Title}");

                article.Title = title;
                article.Keywords = keywords;
                article.Summary = summary;
                article.Content = content;
                article.Link = link;

                await _store.UpdateAsync(article);
            }

            return new ToolResult<ApiReferenceSummary>(
                new ApiReferenceSummary(article.Id, article.Title, article.Summary));
        }

        [KernelFunction("delete")]
        [Description(@"Delete a outdated or redundant API Reference document.")]
        public async Task<ToolResult> DeleteApiReference([Description("The id of the specification to update. Mandatory.")] string id)
        {
            var article = await _store.FindAsync(x => x.Id == id);
            if (article == null) return new ToolResult(true, "Unable to find document");
            await _context.LogInfo($"Deleting API Reference: {article.Title}");
            await _store.DeleteAsync(article);
            return new ToolResult(true, $"API Reference {id} deleted");
        }

        static string Normalize(string s)
        => s.Trim().ToLowerInvariant();
    }
}
