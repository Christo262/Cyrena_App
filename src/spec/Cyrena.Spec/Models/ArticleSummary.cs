namespace Cyrena.Spec.Models
{
    public record ArticleSummary(string Id, string? Title, string? Summary)
    {
        public int Score { get; set; }
    }

    public record NewArticle(string Id, string? Title, string? Summary);
}
