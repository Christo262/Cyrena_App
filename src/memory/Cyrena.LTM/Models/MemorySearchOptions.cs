namespace Cyrena.LTM.Models
{
    /// <summary>
    /// Options for searching long-term memories.
    /// </summary>
    public class MemorySearchOptions
    {
        /// <summary>
        /// Keywords to search for. Memories matching more keywords receive a higher search score.
        /// </summary>
        public string[] Keywords { get; set; } = [];

        /// <summary>
        /// Optional category ID to restrict the search to a specific category.
        /// When null, searches across all categories.
        /// </summary>
        public string? CategoryId { get; set; }

        /// <summary>
        /// Maximum number of results to return. Null returns all results.
        /// </summary>
        public int? MaxResults { get; set; }

        /// <summary>
        /// Minimum relevance score threshold. Results below this score are excluded.
        /// Relevance is a value between 0 and 1.
        /// </summary>
        public double? MinRelevance { get; set; }
    }
}
