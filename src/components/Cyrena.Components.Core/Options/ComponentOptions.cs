using Microsoft.AspNetCore.Components;

namespace Cyrena.Options
{
    public class ComponentOptions
    {
        public ComponentOptions()
        {
            SettingsComponents = new List<Type>();
        }
        internal List<Type> SettingsComponents { get; set; }

        public Type[] GetSettingsComponents()
        {
            return SettingsComponents.ToArray();
        }
    }

    public static class ComponentOptionsExtensions
    {
        public static void AddSettingsComponent<TComponent>(this ComponentOptions options)
            where TComponent : ComponentBase
        {
            if (!options.SettingsComponents.Contains(typeof(TComponent)))
                options.SettingsComponents.Add(typeof(TComponent));
        }
    }
}
