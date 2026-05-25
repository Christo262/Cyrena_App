using Cyrena.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Cyrena.Runtime.OpenAI.Models
{
    public class OpenAIModel : Entity
    {
        [Required]
        public string? ModelId { get; set; }
        [Required]
        public string? DisplayName { get; set; }
        //0-2
        public double Temperature { get; set; } = 1;
        //0-1
        public double TopP { get; set; } = 1;
        public bool SupportFiles { get; set; } = true;
        public bool SupportImages { get; set; } = true;

        [JsonIgnore]
        public int MTemperature
        {
            get
            {
                return (int)(Temperature * 10);
            }
            set
            {
                Temperature = (double)value / 10f;
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
                TopP = (double)value / 100f;
            }
        }
    }
}
