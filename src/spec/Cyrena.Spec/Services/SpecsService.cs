using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Spec.Contracts;
using Cyrena.Spec.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Spec.Services
{
    internal class SpecsService : ISpecsService
    {
        private readonly string _base;
        private readonly IDeveloperContext _context;
        public SpecsService(IDeveloperContext context)
        {
            _context = context;
            _base = Path.Combine(context.Project.RootDirectory, "specs");
            _articles = new List<Article>();
            IndexDirectory(_base);
        }

        public IReadOnlyList<Article> Articles => _articles;

        public IEnumerable<ArticleSummary> Search(string[] keywords, int maxResults)
        {
            _context.LogInfo("Searching specifications");
            var normalized = keywords
                .Select(Normalize)
                .Distinct()
                .ToArray();

            var results = new List<ArticleSummary>();

            foreach (var a in _articles)
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

        public string Read(string id)
        {
            var article = _articles.FirstOrDefault(a => a.Id == id);
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

            if (!string.IsNullOrEmpty(article.FilePath))
            {
                var path = Path.Combine(_base, article.FilePath);
                if (File.Exists(path))
                {
                    sb.AppendLine("[CONTENT]");
                    sb.AppendLine(File.ReadAllText(path));
                    sb.AppendLine("[/CONTENT]");
                    sb.AppendLine();
                }
            }

            if (!string.IsNullOrEmpty(article.Link))
            {
                sb.AppendLine($"[LINK]{article.Link}[/LINK]");
                sb.AppendLine();
            }
            sb.AppendLine("[DOCUMENT END]");
            return sb.ToString();
        }

        public ToolResult<NewArticle> CreateOrUpdateForFile(string id, string? title, string[]? keywords, string? summary, string? content)
        {
            if (!_context.ProjectPlan.TryFindFile(id, out var file))
                return new ToolResult<NewArticle>(false, $"Unable to find file with id {id}");
            var article = _articles.FirstOrDefault(a => a.Id == id);
            if (article == null)
            {
                if (title == null || keywords == null || summary == null || content == null)
                    return new ToolResult<NewArticle>(false, "Requires title, keywords, summary & content in order to create.");
                return Create(title, keywords, summary, content, id);
            }
            return Update(id, title, keywords, summary, content);
        }

        public ToolResult<NewArticle> Create(string title, string[] keywords, string summary, string content, string? id = null)
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
                SpecPath = Path.Combine(_base, $"{id}.spec")
            };
            Dir();
            var path = Path.Combine(_base, $"{article.Id}.spec");
            File.WriteAllText(path, article.ToString());
            var model = new NewArticle(article.Id, article.Title, article.Summary);
            return new ToolResult<NewArticle>(model);
        }

        public ToolResult<NewArticle> Update(string id, string? title, string[]? keywords, string? summary, string? content)
        {
            var article = _articles.FirstOrDefault(a => a.Id == id);
            if (article == null) return new ToolResult<NewArticle>(false, "Unable to find document");

            _context.LogInfo($"Updating specification: {article.Title}");
            if (!string.IsNullOrEmpty(title)) article.Title = title;
            if (!string.IsNullOrEmpty(summary)) article.Summary = summary;
            if (!string.IsNullOrEmpty(content)) article.Content = content;
            if (keywords != null && keywords.Length > 0) article.Keywords = keywords.ToList();

            File.WriteAllText(article.SpecPath, article.ToString());
            return new ToolResult<NewArticle>(new NewArticle(article.Id, article.Title, article.Summary));
        }

        public ToolResult Delete(string id)
        {
            var article = _articles.FirstOrDefault(b => b.Id == id);
            if (article == null) return new ToolResult(true, "Document not found.");
            _context.LogInfo($"Deleting specification: {article.Title}");
            if (File.Exists(article.SpecPath))
                File.Delete(article.SpecPath);
            _articles.Remove(article);
            return new ToolResult(true, "Document Removed");
        }

        private List<Article> _articles;
        private void IndexDirectory(string directory)
        {
            if (!Directory.Exists(directory)) return;
            var files = Directory.GetFiles(directory, "*.spec");
            foreach (var file in files)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var article = JsonConvert.DeserializeObject<Article>(json);
                    if (article != null && !_articles.Any(x => x.Id == article.Id))
                    {
                        article.SpecPath = file;
                        _articles.Add(article);
                    }
                }
                catch { }
            }
            var dirs = Directory.GetDirectories(directory);
            foreach (var dir in dirs)
                IndexDirectory(dir);
        }

        public void Update(Article article)
        {
            File.WriteAllText(article.SpecPath, article.ToString());
        }

        public void Create(Article article)
        {
            article.Id = Guid.NewGuid().ToString();
            Dir();
            var path = Path.Combine(_base, $"{article.Id}.spec");
            File.WriteAllText(path, article.ToString());
            article.SpecPath = path;
            _articles.Add(article);
        }

        public void Delete(Article article)
        {
            if (File.Exists(article.SpecPath))
                File.Delete(article.SpecPath);
            _articles.Remove(article);
        }

        private void Dir()
        {
            if (!Directory.Exists(_base))
                Directory.CreateDirectory(_base);
        }

        static string Normalize(string s)
                => s.Trim().ToLowerInvariant();
    }
}
