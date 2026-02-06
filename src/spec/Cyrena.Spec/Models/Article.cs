using Cyrena.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Cyrena.Spec.Models
{
    public class Article : JsonStringObject
    {
        public Article()
        {
            Keywords = new List<string>();
        }

        public string Id { get; set; } = default!;
        [Required]
        public string? Title { get; set; }
        public string? Summary { get; set; }
        public List<string> Keywords { get; set; }

        /// <summary>
        /// If content is directly saved here
        /// </summary>
        public string? Content { get; set; }
        /// <summary>
        /// If the documentation is online
        /// </summary>
        public string? Link { get; set; }
        /// <summary>
        /// If documentation is in a corresponding file. Must be relative path from spec folder, i.e. http_apis/users.spec, or overview.spec
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>
        /// Leave alone, spec service will assign this value
        /// </summary>
        [JsonIgnore]
        internal string SpecPath { get; set; } = default!;
    }
}
