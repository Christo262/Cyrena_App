using Cyrena.Contracts;
using Cyrena.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        [Obsolete("Use new section mapping API")]
        public static CyrenaBuilder AddSettingsComponent<TComponent>(this CyrenaBuilder builder)
            where TComponent : ComponentBase
        {
            ComponentOptions ui = builder.GetFeatureOption<ComponentOptions>();
            ui.AddSettingsComponent<TComponent>();
            return builder;
        }

        public static CyrenaBuilder AddSettingsComponent<TComponent>(this CyrenaBuilder builder, string section)
            where TComponent : ComponentBase
        {
            ComponentOptions ui = builder.GetFeatureOption<ComponentOptions>();
            ui.AddSettingsComponent<TComponent>(section);
            return builder;
        }

        public static CyrenaBuilder AddSettingsComponent<TComponent>(this CyrenaBuilder builder, string section, int order)
            where TComponent : ComponentBase
        {
            ComponentOptions ui = builder.GetFeatureOption<ComponentOptions>();
            ui.AddSettingsComponent<TComponent>(section, order);
            return builder;
        }

        public static CyrenaBuilder AddShortcut<TShortcut>(this CyrenaBuilder builder)
            where TShortcut : class, IShortcut
        {
            builder.Services.AddScoped<IShortcut, TShortcut>();
            return builder;
        }
    }
}
