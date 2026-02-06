using Cyrena.Contracts;
using Microsoft.AspNetCore.Components;

namespace Cyrena.Models
{
    public abstract class DevelopComponentBase : CyrenaComponentBase
    {
        [Parameter]
        [EditorRequired]
        public IDeveloperContext Context { get; set; } = default!;
    }
}
