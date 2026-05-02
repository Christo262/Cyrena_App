using System.ComponentModel.DataAnnotations;

namespace Cyrena.Runtime.OpenAI.Options
{
    /// <summary>
    /// OpenAI configuration
    /// </summary>
    public class OpenAIOptions
    {
        public const string Key = "openai";

        [Required]
        public string? ApiKey { get; set; }
    }
}