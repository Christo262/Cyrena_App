using Cyrena.Contracts;
using Cyrena.Options;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        public static void AddFeatureOption<T>(this CyrenaBuilder builder, T option)
            where T : class
        {
            var n = typeof(T).Name;
            if (builder.FeatureOptions.ContainsKey(n))
                throw new InvalidOperationException($"{n} already added to Feature Options");
            builder.FeatureOptions[n] = option;
        }

        public static object? GetFeatureOption(this CyrenaBuilder builder, string name)
        {
            if (builder.FeatureOptions.ContainsKey(name)) return builder.FeatureOptions[name];
            return null;
        }

        public static T GetFeatureOption<T>(this CyrenaBuilder builder) where T : class
        {
            var n = typeof(T).Name;
            var obj = builder.GetFeatureOption(n);
            if (obj is T t) return t;
            throw new NullReferenceException($"{n} not present in Feature Options");
        }

        public static void AddFeatureAssembly(this CyrenaBuilder builder, string key, Assembly assembly)
        {
            if (builder.FeatureAssemblies.ContainsKey(key))
                builder.FeatureAssemblies[key].Add(assembly);
            else
                builder.FeatureAssemblies.Add(key, new List<Assembly> { assembly });
        }

        public static void AddFeatureAssembly<T>(this CyrenaBuilder builder, string key)
        {
            var ass = typeof(T).Assembly;
            builder.AddFeatureAssembly(key, ass);
        }

        public static IList<Assembly> GetFeatureAssemblies(this CyrenaOptions builder, string key)
        {
            if (builder.FeatureAssemblies.ContainsKey(key))
                return builder.FeatureAssemblies[key];
            return new List<Assembly>();
        }

        public static void AddStartupTask<TStartupTask>(this CyrenaBuilder builder) where TStartupTask : class, IStartupTask
        {
            builder.Services.AddSingleton<IStartupTask, TStartupTask>();
        }

        public static void AddAssistantMode<TAssistantMode>(this CyrenaBuilder builder)
            where TAssistantMode : class, IAssistantMode
        {
            builder.Services.AddSingleton<IAssistantMode, TAssistantMode>();
        }

        public static void AddAssistantPlugin<TAssistantPlugin>(this CyrenaBuilder builder)
            where TAssistantPlugin : class, IAssistantPlugin
        {
            builder.Services.AddSingleton<IAssistantPlugin, TAssistantPlugin>();
        }
    }
}
