using Microsoft.SemanticKernel;

namespace Cyrena.Models
{
    public class AdditionalMessageContent
    {
        public AdditionalMessageContent(string name, KernelContent item)
        {
            Name = name;
            Item = item;
        }

        public string Name { get; set; }
        public KernelContent Item { get; set; }
    }
}
