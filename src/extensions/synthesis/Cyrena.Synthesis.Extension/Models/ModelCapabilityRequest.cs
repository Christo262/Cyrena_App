using System.ComponentModel.DataAnnotations;

namespace Cyrena.Synthesis.Models
{
    /// <summary>
    /// Used to create a new CapabilityBuilderAssistant to build a new capability
    /// </summary>
    public class ModelCapabilityRequest
    {
        public ModelCapabilityRequest(string title, string instruction)
        {
            Title = title;
            Instruction = instruction;
        }

        [Required]
        public string Title { get; set; }
        [Required]
        public string Instruction { get; set; }
    }
}
