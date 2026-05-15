using Cyrena.Extensions;
using Cyrena.LTM.Contracts;
using Cyrena.LTM.Models;
using Cyrena.Persistence.Contracts;

namespace Cyrena.LTM.Services
{
    /// <summary>
    /// Service for managing long-term memory categories and entries.
    /// Provides CRUD operations and keyword-based search with relevance scoring
    /// that combines search match quality with temporal decay.
    /// 
    /// <b>Decay Behavior:</b> Memories automatically expire based on their category's decay setting.
    /// When a decayed memory is encountered during search, retrieval, or listing, it is
    /// immediately deleted. The <see cref="DeleteDecayedEntriesAsync"/> method can also
    /// be called explicitly to purge all decayed memories.
    /// </summary>
    public class MemoryService : IMemoryService
    {
        private readonly IStore<Category> _categories;
        private readonly IStore<Entry> _entries;

        public MemoryService(IStore<Category> categories, IStore<Entry> entries)
        {
            _categories = categories;
            _entries = entries;
        }

        #region Categories

        /// <inheritdoc />
        public Task<IEnumerable<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default)
            => _categories.FindManyAsync(c => true, ct: cancellationToken);

        /// <inheritdoc />
        public Task<Category?> GetCategoryAsync(string categoryId, CancellationToken cancellationToken = default)
            => _categories.FindAsync(c => c.Id == categoryId, cancellationToken);

        /// <inheritdoc />
        public Task<Category?> GetCategoryByNameAsync(string name, CancellationToken cancellationToken = default)
            => _categories.FindAsync(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase), cancellationToken);

        /// <inheritdoc />
        public async Task<Category> CreateCategoryAsync(string name, string? description = null, CategoryDecay decay = CategoryDecay.Normal, CancellationToken cancellationToken = default)
        {
            var category = new Category
            {
                Name = name,
                Description = description,
                Decay = decay
            };

            await _categories.AddAsync(category, cancellationToken);
            return category;
        }

        /// <inheritdoc />
        public Task UpdateCategoryAsync(Category category, CancellationToken cancellationToken = default)
            => _categories.UpdateAsync(category, cancellationToken);

        /// <inheritdoc />
        public async Task DeleteCategoryAsync(string categoryId, CancellationToken cancellationToken = default)
        {
            var category = await GetCategoryAsync(categoryId, cancellationToken);
            if (category is not null)
            {
                await _categories.DeleteAsync(category, cancellationToken);
                await _entries.DeleteManyAsync(x => x.CategoryId == categoryId, cancellationToken);
            }
        }

        #endregion

        #region Entries

        /// <inheritdoc />
        public async Task<Entry?> GetEntryAsync(string entryId, CancellationToken cancellationToken = default)
        {
            var entry = await _entries.FindAsync(e => e.Id == entryId, cancellationToken);
            if (entry is null)
                return null;

            // Auto-delete if decayed
            var category = await GetCategoryAsync(entry.CategoryId, cancellationToken);
            if (category is not null && IsDecayed(entry, category))
            {
                await DeleteEntryAsync(entryId, cancellationToken);
                return null;
            }

            return entry;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Entry>> GetEntriesByCategoryAsync(string categoryId, CancellationToken cancellationToken = default)
        {
            var entries = await _entries.FindManyAsync(e => e.CategoryId == categoryId, ct: cancellationToken);
            var category = await GetCategoryAsync(categoryId, cancellationToken);
            if (category is null)
                return entries; // Category missing, can't check decay — return as-is

            var aliveEntries = new List<Entry>();
            foreach (var entry in entries)
            {
                if (IsDecayed(entry, category))
                {
                    await DeleteEntryAsync(entry.Id, cancellationToken);
                }
                else
                {
                    aliveEntries.Add(entry);
                }
            }
            return aliveEntries;
        }

        /// <inheritdoc />
        public async Task<Entry> CreateEntryAsync(
            string categoryId,
            string title,
            string? description = null,
            string[]? keywords = null,
            CancellationToken cancellationToken = default)
        {
            var entry = new Entry
            {
                CategoryId = categoryId,
                Title = title,
                Description = description,
                Keywords = keywords ?? [],
                Facts = []
            };

            await _entries.AddAsync(entry, cancellationToken);
            return entry;
        }

        /// <inheritdoc />
        public Task UpdateEntryAsync(Entry entry, CancellationToken cancellationToken = default)
            => _entries.UpdateAsync(entry, cancellationToken);

        /// <inheritdoc />
        public async Task DeleteEntryAsync(string entryId, CancellationToken cancellationToken = default)
        {
            var entry = await _entries.FindAsync(e => e.Id == entryId, cancellationToken);
            if (entry is not null)
            {
                await _entries.DeleteAsync(entry, cancellationToken);
            }
        }
        #endregion

        #region Facts

        /// <inheritdoc />
        public async Task AddFactToEntryAsync(string entryId, MemoryFact fact, CancellationToken cancellationToken = default)
        {
            var entry = await GetEntryAsync(entryId, cancellationToken);
            if (entry is null)
                throw new ArgumentException($"Entry not found: {entryId}", nameof(entryId));

            entry.Facts.Add(fact);
            await _entries.UpdateAsync(entry, cancellationToken);
        }

        /// <inheritdoc />
        public async Task UpdateFactAsync(string entryId, string factId, MemoryFact updatedFact, CancellationToken cancellationToken = default)
        {
            var entry = await GetEntryAsync(entryId, cancellationToken);
            if (entry is null)
                throw new ArgumentException($"Entry not found: {entryId}", nameof(entryId));

            var existingFact = entry.Facts.FirstOrDefault(f => f.Id == factId);
            if (existingFact is null)
                throw new ArgumentException($"Fact not found: {factId} in entry {entryId}", nameof(factId));

            existingFact.FactType = updatedFact.FactType;
            existingFact._properties = new Dictionary<string, string?>(updatedFact._properties);
            await _entries.UpdateAsync(entry, cancellationToken);
        }

        /// <inheritdoc />
        public async Task RemoveFactAsync(string entryId, string factId, CancellationToken cancellationToken = default)
        {
            var entry = await GetEntryAsync(entryId, cancellationToken);
            if (entry is null)
                throw new ArgumentException($"Entry not found: {entryId}", nameof(entryId));

            var fact = entry.Facts.FirstOrDefault(f => f.Id == factId);
            if (fact is not null)
            {
                entry.Facts.Remove(fact);
                await _entries.UpdateAsync(entry, cancellationToken);
            }
        }

        #endregion

        #region Merge

        /// <inheritdoc />
        public async Task<Entry> MergeEntriesAsync(string targetEntryId, string sourceEntryId, CancellationToken cancellationToken = default)
        {
            if (targetEntryId == sourceEntryId)
                throw new ArgumentException("Cannot merge an entry with itself.", nameof(sourceEntryId));

            var target = await GetEntryAsync(targetEntryId, cancellationToken);
            if (target is null)
                throw new ArgumentException($"Target entry not found: {targetEntryId}", nameof(targetEntryId));

            var source = await GetEntryAsync(sourceEntryId, cancellationToken);
            if (source is null)
                throw new ArgumentException($"Source entry not found: {sourceEntryId}", nameof(sourceEntryId));

            // Merge facts: move all source facts to target
            foreach (var fact in source.Facts)
            {
                target.Facts.Add(fact);
            }

            // Merge keywords: union of both sets
            var mergedKeywords = target.Keywords
                .Union(source.Keywords, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            target.Keywords = mergedKeywords;

            // Update title if source has a more descriptive one (longer)
            if (source.Title.Length > target.Title.Length)
                target.Title = source.Title;

            // Merge descriptions
            if (!string.IsNullOrWhiteSpace(source.Description))
            {
                if (string.IsNullOrWhiteSpace(target.Description))
                    target.Description = source.Description;
                else if (!target.Description.Contains(source.Description, StringComparison.OrdinalIgnoreCase))
                    target.Description = $"{target.Description} | {source.Description}";
            }

            // Save target and delete source
            await _entries.UpdateAsync(target, cancellationToken);
            await _entries.DeleteAsync(source, cancellationToken);

            return target;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<(Entry EntryA, Entry EntryB, double Similarity)>> FindPotentialDuplicatesAsync(
            string? categoryId = null, double minSimilarity = 0.6, CancellationToken cancellationToken = default)
        {
            var entries = categoryId is not null
                ? await _entries.FindManyAsync(e => e.CategoryId == categoryId, ct: cancellationToken)
                : await _entries.FindManyAsync(e => true, ct: cancellationToken);

            var categories = await _categories.FindManyAsync(c => true, ct: cancellationToken);
            var categoryMap = categories.ToDictionary(c => c.Id);

            var aliveEntries = new List<Entry>();
            foreach (var entry in entries)
            {
                if (categoryMap.TryGetValue(entry.CategoryId, out var cat) && !IsDecayed(entry, cat))
                    aliveEntries.Add(entry);
            }

            var duplicates = new List<(Entry, Entry, double)>();

            for (int i = 0; i < aliveEntries.Count; i++)
            {
                for (int j = i + 1; j < aliveEntries.Count; j++)
                {
                    var a = aliveEntries[i];
                    var b = aliveEntries[j];

                    // Only compare entries in the same category
                    if (a.CategoryId != b.CategoryId)
                        continue;

                    var similarity = CalculateEntrySimilarity(a, b);
                    if (similarity >= minSimilarity)
                    {
                        duplicates.Add((a, b, similarity));
                    }
                }
            }

            return duplicates.OrderByDescending(d => d.Item3);
        }

        /// <summary>
        /// Calculates similarity between two entries based on keyword overlap, title overlap, and fact type overlap.
        /// Returns a value between 0 and 1.
        /// </summary>
        private static double CalculateEntrySimilarity(Entry a, Entry b)
        {
            var aKeywords = a.Keywords.Select(k => k.ToLowerInvariant()).ToHashSet();
            var bKeywords = b.Keywords.Select(k => k.ToLowerInvariant()).ToHashSet();

            double keywordScore = 0;
            if (aKeywords.Count > 0 || bKeywords.Count > 0)
            {
                var intersection = aKeywords.Intersect(bKeywords).Count();
                var union = aKeywords.Union(bKeywords).Count();
                keywordScore = union > 0 ? (double)intersection / union : 0;
            }

            var aTitleWords = a.Title.ToLowerInvariant()
                .Split([' ', '-', '_', '.', ','], StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 2)
                .ToHashSet();
            var bTitleWords = b.Title.ToLowerInvariant()
                .Split([' ', '-', '_', '.', ','], StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 2)
                .ToHashSet();

            double titleScore = 0;
            if (aTitleWords.Count > 0 || bTitleWords.Count > 0)
            {
                var intersection = aTitleWords.Intersect(bTitleWords).Count();
                var union = aTitleWords.Union(bTitleWords).Count();
                titleScore = union > 0 ? (double)intersection / union : 0;
            }

            var aFactTypes = a.Facts.Select(f => f.FactType.ToLowerInvariant()).ToHashSet();
            var bFactTypes = b.Facts.Select(f => f.FactType.ToLowerInvariant()).ToHashSet();

            double factTypeScore = 0;
            if (aFactTypes.Count > 0 || bFactTypes.Count > 0)
            {
                var intersection = aFactTypes.Intersect(bFactTypes).Count();
                var union = aFactTypes.Union(bFactTypes).Count();
                factTypeScore = union > 0 ? (double)intersection / union : 0;
            }

            // Weighted combination: keywords 40%, title 35%, fact types 25%
            return (keywordScore * 0.4) + (titleScore * 0.35) + (factTypeScore * 0.25);
        }

        #endregion

        #region Decay

        /// <summary>
        /// Decay thresholds in days for each decay rate.
        /// A memory is considered decayed when its age exceeds this threshold.
        /// </summary>
        private static readonly Dictionary<CategoryDecay, double> DecayThresholdDays = new()
        {
            [CategoryDecay.Fast] = 7.0,
            [CategoryDecay.Normal] = 30.0,
            [CategoryDecay.Slow] = 90.0,
            [CategoryDecay.None] = double.MaxValue
        };

        /// <inheritdoc />
        public bool IsDecayed(Entry entry, Category category)
        {
            if (category.Decay == CategoryDecay.None)
                return false;

            if (!Ulid.TryParse(entry.Id, out var ulid))
                return false; // Can't determine age — assume alive

            var entryTime = ulid.Time;
            var age = DateTimeOffset.UtcNow - entryTime;

            if (DecayThresholdDays.TryGetValue(category.Decay, out var threshold))
            {
                return age.TotalDays > threshold;
            }

            return false; // Unknown decay rate — assume alive
        }

        /// <inheritdoc />
        public async Task<int> DeleteDecayedEntriesAsync(CancellationToken cancellationToken = default)
        {
            var allEntries = await _entries.FindManyAsync(e => true, ct: cancellationToken);
            var categories = await _categories.FindManyAsync(c => true, ct: cancellationToken);
            var categoryMap = categories.ToDictionary(c => c.Id);

            int deletedCount = 0;

            foreach (var entry in allEntries)
            {
                if (!categoryMap.TryGetValue(entry.CategoryId, out var category))
                    continue; // Orphaned entry — skip or could delete it

                if (IsDecayed(entry, category))
                {
                    await _entries.DeleteAsync(entry, cancellationToken);
                    deletedCount++;
                }
            }

            return deletedCount;
        }

        #endregion

        #region Search

        /// <inheritdoc />
        public async Task<IEnumerable<MemorySearchResult>> SearchAsync(MemorySearchOptions options, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(options);

            var searchKeywords = options.Keywords
                .Select(k => k.Trim().ToLowerInvariant())
                .Where(k => !string.IsNullOrEmpty(k))
                .Distinct()
                .ToArray();

            // Load all entries (filtered by category if specified)
            var entries = options.CategoryId is not null
                ? await _entries.FindManyAsync(e => e.CategoryId == options.CategoryId, ct: cancellationToken)
                : await _entries.FindManyAsync(e => true, ct: cancellationToken);

            // Load all categories for decay lookup
            var categories = await _categories.FindManyAsync(c => true, ct: cancellationToken);
            var categoryMap = categories.ToDictionary(c => c.Id);

            var results = new List<MemorySearchResult>();
            int decayedCount = 0;

            foreach (var entry in entries)
            {
                if (!categoryMap.TryGetValue(entry.CategoryId, out var category))
                    continue;

                // Auto-delete decayed memories during search
                if (IsDecayed(entry, category))
                {
                    await DeleteEntryAsync(entry.Id, cancellationToken);
                    decayedCount++;
                    continue;
                }

                var searchScore = CalculateSearchScore(entry, searchKeywords);
                var decayScore = CalculateDecayScore(entry, category);
                var relevanceScore = CalculateRelevanceScore(searchScore, decayScore);

                // Skip if below minimum relevance threshold
                if (options.MinRelevance.HasValue && relevanceScore < options.MinRelevance.Value)
                    continue;

                results.Add(new MemorySearchResult
                {
                    Entry = entry,
                    Category = category,
                    SearchScore = searchScore,
                    DecayScore = decayScore,
                    RelevanceScore = relevanceScore
                });
            }

            // Order by relevance descending
            var orderedResults = results
                .OrderByDescending(r => r.RelevanceScore)
                .ThenByDescending(r => r.SearchScore)
                .ThenByDescending(r => r.DecayScore);

            // Apply max results limit if specified
            if (options.MaxResults.HasValue && options.MaxResults.Value > 0)
            {
                return orderedResults.Take(options.MaxResults.Value);
            }

            return orderedResults;
        }

        #endregion

        #region Scoring

        /// <summary>
        /// Calculates the search score (0 to 1) based on keyword matches.
        /// Score is the ratio of matched keywords to total search keywords.
        /// Matches are checked against: keywords, title, description, fact types, and fact property values.
        /// </summary>
        private static double CalculateSearchScore(Entry entry, string[] searchKeywords)
        {
            if (searchKeywords.Length == 0)
                return 1.0;

            var entryKeywords = entry.Keywords
                .Select(k => k.Trim().ToLowerInvariant())
                .Where(k => !string.IsNullOrEmpty(k))
                .ToHashSet();

            // Also include title and description in the searchable text
            var titleWords = entry.Title
                .Split([' ', '-', '_', '.', ','], StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.Trim().ToLowerInvariant())
                .Where(w => w.Length > 2); // Filter out very short words

            var descriptionWords = (entry.Description ?? string.Empty)
                .Split([' ', '-', '_', '.', ','], StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.Trim().ToLowerInvariant())
                .Where(w => w.Length > 2);

            // Collect all searchable terms from the entry metadata
            var searchableTerms = new HashSet<string>(entryKeywords);
            foreach (var word in titleWords)
                searchableTerms.Add(word);
            foreach (var word in descriptionWords)
                searchableTerms.Add(word);

            // Also scan fact types and fact property values — critical for finding memories
            foreach (var fact in entry.Facts)
            {
                if (!string.IsNullOrEmpty(fact.FactType))
                {
                    var typeWords = fact.FactType
                        .Split([' ', '-', '_', '.', ','], StringSplitOptions.RemoveEmptyEntries)
                        .Select(w => w.Trim().ToLowerInvariant())
                        .Where(w => w.Length > 2);
                    foreach (var word in typeWords)
                        searchableTerms.Add(word);
                }

                foreach (var propValue in fact.Values.Where(v => !string.IsNullOrEmpty(v)))
                {
                    var propWords = propValue!
                        .Split([' ', '-', '_', '.', ','], StringSplitOptions.RemoveEmptyEntries)
                        .Select(w => w.Trim().ToLowerInvariant())
                        .Where(w => w.Length > 2);
                    foreach (var word in propWords)
                        searchableTerms.Add(word);
                }
            }

            int matches = searchKeywords.Count(sk => searchableTerms.Any(st => st.Contains(sk) || sk.Contains(st)));

            return (double)matches / searchKeywords.Length;
        }

        /// <summary>
        /// Calculates the decay score (0 to 1) based on the age of the memory.
        /// Uses the ULID timestamp to determine age and applies category-specific decay.
        /// 
        /// <b>Note:</b> This is the <i>score</i> used for ranking. The actual <i>threshold</i>
        /// for deletion is handled by <see cref="IsDecayed"/>, which uses hard day limits.
        /// </summary>
        private static double CalculateDecayScore(Entry entry, Category category)
        {
            if (!Ulid.TryParse(entry.Id, out var ulid))
                return 0.5; // Fallback if ID is not a valid ULID

            var entryTime = ulid.Time;
            var age = DateTimeOffset.UtcNow - entryTime;

            // Decay rates: how quickly memories lose relevance
            // These are tuned so that:
            // - Fast: significant decay after ~7 days
            // - Normal: significant decay after ~30 days
            // - Slow: significant decay after ~90 days
            // - None: never decay
            double halfLifeDays = category.Decay switch
            {
                CategoryDecay.Fast => 7.0,
                CategoryDecay.Normal => 30.0,
                CategoryDecay.Slow => 90.0,
                CategoryDecay.None => double.MaxValue,
                _ => 30.0
            };

            if (halfLifeDays == double.MaxValue || age.TotalDays <= 0)
                return 1.0;

            // Exponential decay: score = e^(-ln(2) * age / halfLife)
            // At halfLife days, score = 0.5
            // At 2*halfLife days, score = 0.25
            // At 3*halfLife days, score = 0.125
            double decay = Math.Exp(-Math.Log(2) * age.TotalDays / halfLifeDays);

            // Clamp to [0, 1]
            return Math.Clamp(decay, 0.0, 1.0);
        }

        /// <summary>
        /// Calculates the combined relevance score from search score and decay score.
        /// Uses a weighted combination where both factors contribute equally by default.
        /// </summary>
        private static double CalculateRelevanceScore(double searchScore, double decayScore)
        {
            // Equal weighting: 50% search quality, 50% freshness
            // Can be adjusted if search quality or recency should be prioritized
            const double searchWeight = 0.5;
            const double decayWeight = 0.5;

            return (searchScore * searchWeight) + (decayScore * decayWeight);
        }

        #endregion

        public async Task ClearMemoryAsync()
        {
            await _categories.DeleteManyAsync(x => true);
            await _entries.DeleteManyAsync(x => true);
        }
    }
}
