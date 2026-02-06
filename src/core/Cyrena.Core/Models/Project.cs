using System.ComponentModel.DataAnnotations;

namespace Cyrena.Models
{
    public class Project : Entity
    {
        public Project()
        {
            Properties = new DataPropertyCollection();
            Created = DateTime.Now;
            LastModified = DateTime.Now;
        }

        [Required]
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        [Required]
        public string RootDirectory { get; set; } = default!;

        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }

        public string Type { get; set; } = default!;
        [Required]
        public string ConnectionId { get; set; } = default!;

        public DataPropertyCollection Properties { get; set; }
    }
}
