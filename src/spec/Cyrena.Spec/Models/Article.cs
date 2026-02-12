using Cyrena.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Cyrena.Spec.Models
{
    public class Article : JsonStringObject, IEntity
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
    }
}
