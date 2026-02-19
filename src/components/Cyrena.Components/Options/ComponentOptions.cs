using Cyrena.Contracts;
using Microsoft.AspNetCore.Components;
using System.Reflection;

namespace Cyrena.Options
{
    public class ComponentOptions
    {
        public ComponentOptions()
        {
            RouterAssemblies = new List<Assembly>();
            NavigationComponents = new List<Type>();
            SettingsComponents = new List<Type>();
        }
        internal List<Assembly> RouterAssemblies { get; set; }
        internal List<Type> NavigationComponents { get; set; }
        internal List<Type> SettingsComponents { get; set; }

        public Assembly[] GetRouterAssemblies()
        {
            return RouterAssemblies.ToArray();
        }

        public Type[] GetNavigationComponents()
        {
            return NavigationComponents.ToArray();
        }

        public Type[] GetSettingsComponents()
        {
            return SettingsComponents.ToArray();
        }
    }

    public static class ComponentOptionsExtensions
    {
        public static void AddRouterAssembly(this ComponentOptions options, Assembly assembly)
        {
            if (!options.RouterAssemblies.Contains(assembly))
                options.RouterAssemblies.Add(assembly);
        }

        public static void AddRouterAssembly<T>(this ComponentOptions options)
        {
            var assembly = typeof(T).Assembly;
            options.AddRouterAssembly(assembly);
        }

        public static void AddNavigationComponent<TComponent>(this ComponentOptions options)
            where TComponent : ComponentBase
        {
            if (!options.NavigationComponents.Contains(typeof(TComponent)))
                options.NavigationComponents.Add(typeof(TComponent));
        }

        public static void AddSettingsComponent<TComponent>(this ComponentOptions options)
            where TComponent : ComponentBase
        {
            if (!options.SettingsComponents.Contains(typeof(TComponent)))
                options.SettingsComponents.Add(typeof(TComponent));
        }
    }
}
