using Cyrena.Models;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;

namespace Cyrena.APIReferences.Models
{
    public class ApiReference : Entity, ISuppressibleResult
    {
        [Required]
        public string? Title { get; set; }
        public string[] Keywords { get; set; } = [];
        public string? Summary { get; set; }

        public string? Link { get; set; }
        public string? Content { get; set; }

        public string Suppress()
        {
            return $"[APIREF:{Id}; content omitted; use API_reference_read before relying on details]";
        }
    }
}
