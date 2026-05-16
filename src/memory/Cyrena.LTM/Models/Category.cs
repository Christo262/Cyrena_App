using Cyrena.Models;
using System.ComponentModel.DataAnnotations;

namespace Cyrena.LTM.Models
{
    /// <summary>
    /// Represents a LTM category
    /// </summary>
    public class Category : Entity
    {
        public Category()
        {
            Id = Ulid.NewUlid().ToString();
        }

        /// <summary>
        /// The name of the category, i.e. 'personal'
        /// </summary>
        [MaxLength(100)]
        public string Name { get; set; } = default!;
        /// <summary>
        /// A short description of the category memories
        /// </summary>
        [MaxLength(255)]
        public string? Description { get; set; }
        /// <summary>
        /// How fast these memories will decay, affecting relevance score
        /// </summary>
        public CategoryDecay Decay { get; set; } = CategoryDecay.Normal;
    }
}
