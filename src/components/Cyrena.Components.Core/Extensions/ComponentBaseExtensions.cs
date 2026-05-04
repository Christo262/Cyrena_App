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

        public static RenderFragment Render(this ComponentBase cmp, Type type, Dictionary<string, object?> parameters) => builder =>
        {
            builder.OpenComponent(0, type);
            for(int  i = 0; i < parameters.Count; i++)
            {
                var kvp = parameters.ElementAt(i);
                builder.AddAttribute(i + 1, kvp.Key, kvp.Value);
            }
            builder.CloseComponent();
        };
    }
}
