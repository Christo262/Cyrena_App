using Cyrena.Contracts;
using Cyrena.Developer.Docs.Models;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Persistence.Contracts;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text;

namespace Cyrena.Developer.Docs.Plugins
{
    internal class APIReferences
    {
        private readonly IStore<ApiReference> _store;
        private readonly IChatMessageService _context;
        public APIReferences(IStore<ApiReference> store, IChatMessageService chat)
        {
            _store = store;
            _context = chat;
        }

        [KernelFunction("search")]
        [Description("Search API References for authoritative technical documentation about this project. Use this before implementing features to understand APIs, architecture rules, integration contracts, and established behavior.")]
        public async Task<IEnumerable<ApiReferenceSearch>> Search(
            [Description("Keywords describing what specification you are looking for (interfaces, services, architecture, styling, integration, etc.).")] string[] keywords,
            [Description("Maximum number of results to return. Default 10.")] int maxResults = 10)
        {
            await _context.LogInfo("Searching specifications");
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

            return results
                .OrderByDescending(r => r.Score)
                .Take(maxResults);
        }

        [KernelFunction("read")]
        [Description("Read a API Reference document. These documents contain grounded technical specifications about real project code and represent authoritative implementation knowledge.")]
        public async Task<string> Read(
            [Description("The id of the specification document to read.")] string id)
        {
            var article = await _store.FindAsync(x => x.Id == id);
            if (article == null)
                return $"[NOTFOUND]Document with id {id} not found.[/NOTFOUND]";
            await _context.LogInfo($"Reading spec {article.Title}");
            var sb = new StringBuilder();
            sb.AppendLine("[DOCUMENT START]");
            if (!string.IsNullOrEmpty(article.Title))
            {
                sb.AppendLine($"[TITLE]{article.Title}[/TITLE]");
                sb.AppendLine();
            }
            if (!string.IsNullOrEmpty(article.Summary))
            {
                sb.AppendLine("[SUMMARY]");
                sb.AppendLine(article.Summary.Trim());
                sb.AppendLine("[/SUMMARY]");
                sb.AppendLine();
            }

            if (!string.IsNullOrEmpty(article.Content))
            {
                sb.AppendLine("[CONTENT]");
                sb.AppendLine(article.Content.Trim());
                sb.AppendLine("[/CONTENT]");
                sb.AppendLine();
            }

            if (!string.IsNullOrEmpty(article.Link))
            {
                sb.AppendLine($"[LINK]{article.Link}[/LINK]");
                sb.AppendLine();
            }
            sb.AppendLine("[DOCUMENT END]");
            return sb.ToString();
        }

        [KernelFunction("create")]
        [Description(@"Create a new Project Specification document.

                A Project Specification is authoritative technical documentation grounded in actual source code.

                When creating a specification about code:
                1. Read all relevant source files first.
                2. Base the document only on real implementation.
                3. Never write generic or hypothetical descriptions.
                4. Capture real method signatures, contracts, architecture rules, and usage patterns.
                5. The purpose is to guide future implementation accurately.

                This document becomes authoritative project knowledge.")]
        public async Task<ToolResult<ApiReferenceSummary>> Create(
            [Description("Title of the specification document. Mandatory.")] string title,
            [Description("Keywords used to search for this specification in the future. Mandatory.")] string[] keywords,
            [Description("Brief summary of what the specification contains. Mandatory.")] string summary,
            [Description("Grounded technical content in plaintext or markdown. Do not include Title, Summary or Keywords here. Mandatory.")] string content,
            [Description("If the specification is related directly to a file, provide the fileId for linkage. Optional.")] string? fileId = null)
        {
            if(string.IsNullOrEmpty(fileId))
                fileId = Guid.NewGuid().ToString();
            var model = new ApiReference()
            {
                Id = fileId,
                Title = title,
                Keywords = keywords,
                Summary = summary,
                Content = content,
            };
            await _store.SaveAsync(model);
            return new ToolResult<ApiReferenceSummary>(new ApiReferenceSummary(fileId, title, summary));
        }

        [KernelFunction("update")]
        [Description(@"Update an existing Project Specification.

Updates must remain grounded in source code.
If implementation changes, the specification must be revised to match reality.")]
        public async Task<ToolResult<ApiReferenceSummary>> UpdateProjectSpecification(
            [Description("The id of the specification to update. Mandatory.")] string id,
            [Description("New title. Leave null or empty if unchanged. Optional.")] string? title = null,
            [Description("Updated search keywords. Leave null if unchanged. Optional.")] string[]? keywords = null,
            [Description("Updated summary. Leave null or empty if unchanged. Optional.")] string? summary = null,
            [Description("Updated grounded technical content. Do not include Title, Summary or Keywords here. Leave null or empty if unchanged. Optional.")] string? content = null)
        {
            var article = await _store.FindAsync(x => x.Id == id);
            if (article == null) return new ToolResult<ApiReferenceSummary>(false, "Unable to find document");

            await _context.LogInfo($"Updating specification: {article.Title}");
            if (!string.IsNullOrEmpty(title)) article.Title = title;
            if (!string.IsNullOrEmpty(summary)) article.Summary = summary;
            if (!string.IsNullOrEmpty(content)) article.Content = content;
            if (keywords != null && keywords.Length > 0) article.Keywords = keywords;

            await _store.UpdateAsync(article);
            return new ToolResult<ApiReferenceSummary>(new ApiReferenceSummary(article.Id, article.Title, article.Summary));
        }

        static string Normalize(string s)
        => s.Trim().ToLowerInvariant();
    }
}
