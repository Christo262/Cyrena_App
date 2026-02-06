using Microsoft.AspNetCore.Components;

namespace Cyrena.Extensions
{
    public static class ComponentBaseExtensions
    {
        public static RenderFragment Render(this ComponentBase cmp, Type type) => builder =>
        {
            builder.OpenComponent(0, type);
            builder.CloseComponent();
        };
    }
}
