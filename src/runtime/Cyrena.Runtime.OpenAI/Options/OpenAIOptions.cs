using System.ComponentModel.DataAnnotations;

namespace Cyrena.Runtime.OpenAI.Options
{
    /// <summary>
    /// OpenAI configuration
    /// </summary>
    public class OpenAIOptions
    {
        internal const string Key = "openai";

        [Required]
        public string? ApiKey { get; set; }
        [Required]
        public string? ModelId { get; set; }
    }
}

//TODO: Temperature, etc.