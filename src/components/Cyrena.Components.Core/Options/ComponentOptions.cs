using Microsoft.AspNetCore.Components;

namespace Cyrena.Options
{
    public class ComponentOptions
    {
        public ComponentOptions()
        {
            SettingsComponents = new List<ComponentMetaData>();
        }
        internal List<ComponentMetaData> SettingsComponents { get; set; }

        public ComponentMetaData[] GetSettingsComponents()
        {
            return SettingsComponents.ToArray();
        }
    }

    public record ComponentMetaData(Type Component, string? Section);

    public static class ComponentOptionsExtensions
    {
        [Obsolete("Use new section mapping API")]
        public static void AddSettingsComponent<TComponent>(this ComponentOptions options)
            where TComponent : ComponentBase
        {
            if (!options.SettingsComponents.Any(x => x.Component == typeof(TComponent)))
                options.SettingsComponents.Add(new ComponentMetaData(typeof(TComponent), null));
        }

        public static void AddSettingsComponent<TComponent>(this ComponentOptions options, string section)
        {
            if (!options.SettingsComponents.Any(x => x.Component == typeof(TComponent)))
                options.SettingsComponents.Add(new ComponentMetaData(typeof(TComponent), section));
        }
    }
}
