using Cyrena.Models;
using System.Text.Json;

namespace Cyrena.APIReferences.Models
{
    public class ApiReferenceSearch 
    {
        public ApiReferenceSearch(string id, string? title, string? description)
        {
            Id = id;
            Title = title;
            Description = description;
        }

        public string Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int Score { get; set; }
    }
}
