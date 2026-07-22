using Cyrena.Models;
using System.Text.Json;

namespace Cyrena.APIReferences.Models
{
    public class ApiReferenceSummary 
    {
        public ApiReferenceSummary(string id, string? title, string? summary)
        {
            Id = id;
            Title = title;
            Summary = summary;
        }

        public string Id { get; set; }
        public string? Title { get; set; }
        public string? Summary { get; set; }
    }
}
