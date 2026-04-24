using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Cyrena.Extensa.Components.Shared
{
    /// <summary>
    /// Ensures that shared assemblies are loaded to avoid conflicts with ALC
    /// </summary>
    public sealed class Preload : ComponentBase
    {
        protected override void OnInitialized()
        {
            var dummy = typeof(Microsoft.AspNetCore.Components.Forms.InputText).Assembly;
            base.OnInitialized();
        }
    }
}
