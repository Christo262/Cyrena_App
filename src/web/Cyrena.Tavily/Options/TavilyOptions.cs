using System.ComponentModel.DataAnnotations;

namespace Cyrena.Tavily.Options
{
    public class TavilyOptions
    {
        public const string Key = "tavily";
        [Required]
        public string? ApiKey { get; set; }
        public bool Enable { get; set; }
    }
}
