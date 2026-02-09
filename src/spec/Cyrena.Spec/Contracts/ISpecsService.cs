using Cyrena.Models;
using Cyrena.Spec.Models;

namespace Cyrena.Spec.Contracts
{
    public interface ISpecsService
    {
        IReadOnlyList<Article> Articles { get; }
        IEnumerable<ArticleSummary> Search(string[] keywords, int maxResults);
        string Read(string id);
        ToolResult<NewArticle> Create(string title, string[] keywords, string summary, string content, string? id = null);
        ToolResult<NewArticle> Update(string id, string? title, string[]? keywords, string? summary, string? content);
        ToolResult<NewArticle> CreateOrUpdateForFile(string id, string? title, string[]? keywords, string? summary, string? content);
        ToolResult Delete(string id);

        void Update(Article article);
        void Create(Article article);
        void Delete(Article article);
    }
}
