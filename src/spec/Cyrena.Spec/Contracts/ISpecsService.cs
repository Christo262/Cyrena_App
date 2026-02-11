using Cyrena.Models;
using Cyrena.Spec.Models;

namespace Cyrena.Spec.Contracts
{
    public interface ISpecsService
    {
        IQueryable<Article> Articles { get; }
        Task<IEnumerable<ArticleSummary>> Search(string[] keywords, int maxResults);
        Task<string> Read(string id);
        Task<ToolResult<NewArticle>> Create(string title, string[] keywords, string summary, string content, string? id = null);
        Task<ToolResult<NewArticle>> Update(string id, string? title, string[]? keywords, string? summary, string? content);
        Task<ToolResult<NewArticle>> CreateOrUpdateForFile(string id, string? title, string[]? keywords, string? summary, string? content);
        Task<ToolResult> Delete(string id);

        Task Update(Article article);
        Task Create(Article article);
        Task Delete(Article article);
    }
}
