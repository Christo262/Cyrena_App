namespace Cyrena.LTM.Models
{
    /// <summary>
    /// Represents the result of a memory search, including the entry and its computed scores.
    /// </summary>
    public class MemorySearchResult
    {
        /// <summary>
        /// The memory entry that matched the search.
        /// </summary>
        public required Entry Entry { get; set; }

        /// <summary>
        /// The category the entry belongs to.
        /// </summary>
        public required Category Category { get; set; }

        /// <summary>
        /// Search score based on keyword matching (0 to 1).
        /// Higher values indicate more keyword matches.
        /// </summary>
        public double SearchScore { get; set; }

        /// <summary>
        /// Decay score based on the age of the memory and category decay rate (0 to 1).
        /// Higher values indicate fresher memories. Older memories decay toward 0.
        /// </summary>
        public double DecayScore { get; set; }

        /// <summary>
        /// Combined relevance score (0 to 1) calculated from search score and decay score.
        /// Represents the overall relevance of this memory to the search query.
        /// </summary>
        public double RelevanceScore { get; set; }
    }
}
