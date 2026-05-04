using Microsoft.AspNetCore.Components;
using Microsoft.SemanticKernel;

namespace Cyrena.Models
{
    public abstract class KernelComponentBase : ComponentBase
    {
        [Parameter]
        [EditorRequired]
        public Kernel Kernel { get; set; } = default!;
    }
}
