using Microsoft.AspNetCore.Components;
using Microsoft.SemanticKernel;

namespace Cyrena.Models
{
    public abstract class CapabilityComponentBase : ComponentBase
    {
        [Parameter]
        [EditorRequired]
        public Kernel Kernel { get; set; }
        [Parameter]
        public EventCallback<AdditionalMessageContent[]> OnItemsAdded { get; set; }
    }
}
