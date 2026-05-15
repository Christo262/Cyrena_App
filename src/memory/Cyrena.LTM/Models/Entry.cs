using Cyrena.Models;
using System.ComponentModel.DataAnnotations;

namespace Cyrena.LTM.Models
{
    /// <summary>
    /// Represents a memory entry — the searchable envelope that groups related facts.
    /// The entry holds metadata (title, description, keywords) for discovery,
    /// while the actual things to remember live in <see cref="Facts"/>.
    /// </summary>
    public class Entry : Entity
    {
        public Entry()
        {
            Id = Ulid.NewUlid().ToString();
            Facts = new List<MemoryFact>();
        }

        /// <summary>
        /// The category id of the memory
        /// </summary>
        [Required]
        public string CategoryId { get; set; } = default!;

        /// <summary>
        /// Brief title of the memory — the primary human-readable label
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = default!;

        /// <summary>
        /// Keywords for searching and cross-referencing
        /// </summary>
        public string[] Keywords { get; set; } = [];

        /// <summary>
        /// Brief description providing context
        /// </summary>
        [MaxLength(255)]
        public string? Description { get; set; }

        /// <summary>
        /// Structured facts associated with this memory — the actual things to remember.
        /// Each fact has a free-form <see cref="MemoryFact.FactType"/> and typed key-value properties.
        /// </summary>
        public List<MemoryFact> Facts { get; set; }
    }
}
