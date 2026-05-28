using Microsoft.AspNetCore.Components;

namespace Cyrena.Options
{
    public class ComponentOptions
    {
        public ComponentOptions()
        {
            SettingsComponents = new List<ComponentMetaData>();
            MenuOptions = new MenuConfig();
            FileSystemOptions = new FileSystemConfig();
        }
        internal List<ComponentMetaData> SettingsComponents { get; set; }

        public ComponentMetaData[] GetSettingsComponents()
        {
            return SettingsComponents.ToArray();
        }

        public class MenuConfig
        {
            public string ConverseUrl { get; set; } = "converse/{Id}";
            public bool AllowNewTab { get; set; } = true;
        }

        public class FileSystemConfig
        {
            public bool ExploreFolder { get; set; } = true;
        }

        public MenuConfig MenuOptions { get; set; }
        public FileSystemConfig FileSystemOptions { get; set; }

        public static string OllamaDefaultEndpoint { get; set; } = "http://localhost:11434";
        public static bool IsServer { get; set; } = true;
    }

    public record ComponentMetaData(Type Component, string? Section, int Order);

    public static class ComponentOptionsExtensions
    {
        [Obsolete("Use new section mapping API")]
        public static void AddSettingsComponent<TComponent>(this ComponentOptions options)
            where TComponent : ComponentBase
        {
            if (!options.SettingsComponents.Any(x => x.Component == typeof(TComponent)))
                options.SettingsComponents.Add(new ComponentMetaData(typeof(TComponent), null, 10));
        }

        public static void AddSettingsComponent<TComponent>(this ComponentOptions options, string section)
        {
            if (!options.SettingsComponents.Any(x => x.Component == typeof(TComponent)))
                options.SettingsComponents.Add(new ComponentMetaData(typeof(TComponent), section, 10));
        }

        public static void AddSettingsComponent<TComponent>(this ComponentOptions options, string section, int order)
        {
            if (!options.SettingsComponents.Any(x => x.Component == typeof(TComponent)))
                options.SettingsComponents.Add(new ComponentMetaData(typeof(TComponent), section, order));
        }
    }
}
