using Cyrena.LTM.Models;

namespace Cyrena.LTM.Contracts
{
    /// <summary>
    /// Contract for managing long-term memory categories and entries.
    /// Provides CRUD operations for categories and entries, plus keyword-based search with relevance scoring.
    /// Includes automatic decay: memories past their decay threshold are deleted when accessed.
    /// </summary>
    public interface IMemoryService
    {
        // Category operations
        Task<IEnumerable<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default);
        Task<Category?> GetCategoryAsync(string categoryId, CancellationToken cancellationToken = default);
        Task<Category?> GetCategoryByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<Category> CreateCategoryAsync(string name, string? description = null, CategoryDecay decay = CategoryDecay.Normal, CancellationToken cancellationToken = default);
        Task UpdateCategoryAsync(Category category, CancellationToken cancellationToken = default);
        Task DeleteCategoryAsync(string categoryId, CancellationToken cancellationToken = default);

        // Entry operations
        Task<Entry?> GetEntryAsync(string entryId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Entry>> GetEntriesByCategoryAsync(string categoryId, CancellationToken cancellationToken = default);
        Task<Entry> CreateEntryAsync(string categoryId, string title, string? description = null, string[]? keywords = null, CancellationToken cancellationToken = default);
        Task UpdateEntryAsync(Entry entry, CancellationToken cancellationToken = default);
        Task DeleteEntryAsync(string entryId, CancellationToken cancellationToken = default);

        // Fact operations
        Task AddFactToEntryAsync(string entryId, MemoryFact fact, CancellationToken cancellationToken = default);
        Task UpdateFactAsync(string entryId, string factId, MemoryFact updatedFact, CancellationToken cancellationToken = default);
        Task RemoveFactAsync(string entryId, string factId, CancellationToken cancellationToken = default);

        // Merge operations
        /// <summary>
        /// Merges two memory entries into one. The source entry is deleted; its facts are moved to the target.
        /// The target's title, description, and keywords are updated to reflect the merge.
        /// </summary>
        Task<Entry> MergeEntriesAsync(string targetEntryId, string sourceEntryId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds potential duplicate memories based on keyword overlap and same category.
        /// Returns pairs of entries that may represent the same information.
        /// </summary>
        Task<IEnumerable<(Entry EntryA, Entry EntryB, double Similarity)>> FindPotentialDuplicatesAsync(string? categoryId = null, double minSimilarity = 0.6, CancellationToken cancellationToken = default);

        // Decay operations
        /// <summary>
        /// Checks whether a memory entry has decayed past its category's threshold.
        /// </summary>
        bool IsDecayed(Entry entry, Category category);

        /// <summary>
        /// Deletes all memory entries that have decayed past their category's threshold.
        /// Returns the number of entries deleted.
        /// </summary>
        Task<int> DeleteDecayedEntriesAsync(CancellationToken cancellationToken = default);

        // Search
        Task<IEnumerable<MemorySearchResult>> SearchAsync(MemorySearchOptions options, CancellationToken cancellationToken = default);

        Task ClearMemoryAsync();
    }
}
