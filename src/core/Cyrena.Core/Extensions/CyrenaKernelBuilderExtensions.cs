using Cyrena.Models;

namespace Cyrena.Extensions
{
    public static class CyrenaKernelBuilderExtensions
    {
        public static void AddFeatureOption<T>(this CyrenaKernelBuilder builder, T option)
            where T : class
        {
            var n = typeof(T).Name;
            if (builder.FeatureOptions.ContainsKey(n))
                throw new InvalidOperationException($"{n} already added to Feature Options");
            builder.FeatureOptions[n] = option;
        }

        public static object? GetFeatureOption(this CyrenaKernelBuilder builder, string name)
        {
            if (builder.FeatureOptions.ContainsKey(name)) return builder.FeatureOptions[name];
            return null;
        }

        public static T GetFeatureOption<T>(this CyrenaKernelBuilder builder) where T : class
        {
            var n = typeof(T).Name;
            var obj = builder.GetFeatureOption(n);
            if (obj is T t) return t;
            throw new NullReferenceException($"{n} not present in Feature Options");
        }
    }
}
