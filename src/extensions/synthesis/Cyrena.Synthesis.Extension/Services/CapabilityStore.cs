using Cyrena.Persistence.Contracts;
using Cyrena.Synthesis.Contracts;
using Cyrena.Synthesis.Models;
using Cyrena.Extensions;

namespace Cyrena.Synthesis.Services
{
    /// <summary>
    /// Provides persistence and search capabilities for dynamic capabilities.
    /// Uses IStore<DynamicCapability> for persistence.
    /// </summary>
    internal class CapabilityStore : ICapabilityStore
    {
        private readonly IStore<DynamicCapability> _store;

        public CapabilityStore(IStore<DynamicCapability> store)
        {
            _store = store;
        }

        public async Task<DynamicCapability> SaveAsync(DynamicCapability script, CancellationToken cancellationToken = default)
        {
            script.ModifiedAt = DateTime.UtcNow;
            script.Code = EnsureRequiredFSharpImports(script.Code);

            // Use QueryableData to check if entity exists by Id
            var existing = await _store.FindAsync(e => e.Id == script.Id, cancellationToken);
            if (existing != null)
            {
                script.Version = existing.Version + 1;
                await _store.UpdateAsync(script, cancellationToken);
            }
            else
            {
                await _store.AddAsync(script, cancellationToken);
            }

            return script;
        }

        public Task<DynamicCapability?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return _store.FindAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<DynamicCapability?> GetByTitleAsync(string title, CancellationToken cancellationToken = default)
        {
            var results = await _store.FindManyAsync(
                e => e.Title.Equals(title, StringComparison.OrdinalIgnoreCase),
                ct: cancellationToken);
            return results.FirstOrDefault();
        }

        public async Task<IReadOnlyList<DynamicCapability>> SearchByKeywordsAsync(IEnumerable<string> keywords, CancellationToken cancellationToken = default)
        {
            var keywordSet = keywords.Select(k => k.ToLowerInvariant()).ToHashSet();

            // Use QueryableData for complex LINQ queries
            var results = _store.QueryableData
                .Where(s => s.Keywords.Any(k => keywordSet.Contains(k.ToLowerInvariant())))
                .ToList();

            return results;
        }

        public async Task<IReadOnlyList<DynamicCapability>> SearchByTitleAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            var term = searchTerm.ToLowerInvariant();

            var results = await _store.FindManyAsync(
                e => e.Title.ToLowerInvariant().Contains(term),
                ct: cancellationToken);

            return results.ToList();
        }

        public Task<IReadOnlyList<DynamicCapability>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            // Use QueryableData to get all entities
            var results = _store.QueryableData.ToList();
            return Task.FromResult<IReadOnlyList<DynamicCapability>>(results);
        }

        public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            var script = await _store.FindAsync(e => e.Id == id, cancellationToken);
            if (script == null)
            {
                return false;
            }

            await _store.DeleteAsync(script, cancellationToken);
            return true;
        }

        private static string EnsureRequiredFSharpImports(string code)
        {
            const string requiredImport = "open Cyrena.Synthesis.Contracts";

            if (code.Contains(requiredImport, StringComparison.Ordinal))
            {
                return code;
            }

            return requiredImport + Environment.NewLine + Environment.NewLine + code.TrimStart();
        }
    }
}
