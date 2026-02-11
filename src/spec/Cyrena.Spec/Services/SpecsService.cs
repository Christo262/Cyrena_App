using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Persistence.Contracts;
using Cyrena.Spec.Contracts;
using Cyrena.Spec.Models;
using System.Text;

namespace Cyrena.Spec.Services
{
    internal class SpecsService : ISpecsService
    {
        private readonly IDeveloperContext _context;
        private readonly IStore<Article> _store;
        public SpecsService(IDeveloperContext context, IStore<Article> store)
        {
            _context = context;
            _store = store;
        }

        public IQueryable<Article> Articles => _store.QueryableData;
        public async Task<IEnumerable<ArticleSummary>> Search(string[] keywords, int maxResults)
        {
            _context.LogInfo("Searching specifications");
            var normalized = keywords
                .Select(Normalize)
                .Distinct()
                .ToArray();

            var results = new List<ArticleSummary>();
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
                    results.Add(new ArticleSummary(a.Id, a.Title, a.Summary)
                    {
                        Score = score
                    });
                }
            }

            return results
                .OrderByDescending(r => r.Score)
                .Take(maxResults);
        }

        public async Task<string> Read(string id)
        {
            var article = await _store.FindAsync(x => x.Id == id);
            if (article == null)
                return $"[NOTFOUND]Document with id {id} not found.[/ERROR]";
            _context.LogInfo($"Reading spec {article.Title}");
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

        public async Task<ToolResult<NewArticle>> CreateOrUpdateForFile(string id, string? title, string[]? keywords, string? summary, string? content)
        {
            if (!_context.ProjectPlan.TryFindFile(id, out var file))
                return new ToolResult<NewArticle>(false, $"Unable to find file with id {id}");
            var article = await _store.FindAsync(x => x.Id == id);
            if (article == null)
            {
                if (title == null || keywords == null || summary == null || content == null)
                    return new ToolResult<NewArticle>(false, "Requires title, keywords, summary & content in order to create.");
                return await Create(title, keywords, summary, content, id);
            }
            return await Update(id, title, keywords, summary, content);
        }

        public async Task<ToolResult<NewArticle>> Create(string title, string[] keywords, string summary, string content, string? id = null)
        {
            _context.LogInfo($"Create specification: {title}");
            if(string.IsNullOrEmpty(id))
                id = Guid.NewGuid().ToString(); 
            var article = new Article()
            {
                Id = id,
                Title = title,
                Keywords = keywords.ToList(),
                Summary = summary,
                Content = content,
            };
            await _store.AddAsync(article);
            var model = new NewArticle(article.Id, article.Title, article.Summary);
            return new ToolResult<NewArticle>(model);
        }

        public async Task<ToolResult<NewArticle>> Update(string id, string? title, string[]? keywords, string? summary, string? content)
        {
            var article = await _store.FindAsync(x => x.Id == id);
            if (article == null) return new ToolResult<NewArticle>(false, "Unable to find document");

            _context.LogInfo($"Updating specification: {article.Title}");
            if (!string.IsNullOrEmpty(title)) article.Title = title;
            if (!string.IsNullOrEmpty(summary)) article.Summary = summary;
            if (!string.IsNullOrEmpty(content)) article.Content = content;
            if (keywords != null && keywords.Length > 0) article.Keywords = keywords.ToList();

            await _store.UpdateAsync(article);
            return new ToolResult<NewArticle>(new NewArticle(article.Id, article.Title, article.Summary));
        }

        public async Task<ToolResult> Delete(string id)
        {
            var article = await _store.FindAsync(x => x.Id == id);
            if (article == null) return new ToolResult(true, "Document not found.");
            _context.LogInfo($"Deleting specification: {article.Title}");
            await _store.DeleteAsync(article);
            return new ToolResult(true, "Document Removed");
        }

        public async Task Update(Article article)
        {
            await _store.UpdateAsync(article);
        }

        public async Task Create(Article article)
        {
            article.Id = Guid.NewGuid().ToString();
            await _store.AddAsync(article);
        }

        public async Task Delete(Article article)
        {
            await _store.DeleteAsync(article);
        }

        static string Normalize(string s)
                => s.Trim().ToLowerInvariant();
    }
}
