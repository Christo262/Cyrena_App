using Cyrena.Synthesis.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Cyrena.Synthesis.Contracts
{
    /// <summary>
    /// Provides persistence and search capabilities for dynamic capabilities.
    /// </summary>
    public interface ICapabilityStore
    {
        /// <summary>
        /// Creates a new dynamic capability or updates an existing one.
        /// </summary>
        Task<DynamicCapability> SaveAsync(DynamicCapability script, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a dynamic capability by its unique identifier.
        /// </summary>
        Task<DynamicCapability?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a dynamic capability by its title (exact match).
        /// </summary>
        Task<DynamicCapability?> GetByTitleAsync(string title, CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches dynamic capabilities by keywords. Returns dynamic capabilities where any keyword matches.
        /// </summary>
        Task<IReadOnlyList<DynamicCapability>> SearchByKeywordsAsync(IEnumerable<string> keywords, CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches dynamic capabilities by title (partial match, case-insensitive).
        /// </summary>
        Task<IReadOnlyList<DynamicCapability>> SearchByTitleAsync(string searchTerm, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all dynamic capabilities.
        /// </summary>
        Task<IReadOnlyList<DynamicCapability>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a dynamic capability by its ID.
        /// </summary>
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
    }
}
