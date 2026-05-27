using Cyrena.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Cyrena.Runtime.Ollama.Models
{
    public class OllamaConnectionInfo : Entity
    {
        [Required]
        public string Endpoint { get; set; } = "http://localhost:11434";
        [Required]
        public string ModelId { get; set; } = default!;
        [Required]
        public string Name { get; set; } = default!;
        public float Temperature { get; set; } = 0.2f;
        public int NumPredict { get; set; } = 128;
        public int NumContext { get; set; } = 4096;
        public int TopK { get; set; } = 40;
        public float TopP { get; set; } = 0.9f;
        public float MinP { get; set; } = 0.0f;

        /// <summary>
        /// For direct cloud access only
        /// </summary>
        public string? APIKey { get; set; }

        public string? Thinking { get; set; }

        [JsonIgnore]
        public int MTemperature
        {
            get
            {
                return (int)(Temperature * 10);
            }
            set
            {
                Temperature = (float)value / 10f;
            }
        }

        [JsonIgnore]
        public int MTopP
        {
            get
            {
                return (int)(TopP * 100);
            }
            set
            {
                TopP = (float)value / 100f;
            }
        }

        [JsonIgnore]
        public int MMinP
        {
            get
            {
                return (int)(MinP * 100);
            }
            set
            {
                MinP = (float)value / 100f;
            }
        }

        public bool SupportsImage { get; set; }
        public bool SupportsFile { get; set; }
    }
}
