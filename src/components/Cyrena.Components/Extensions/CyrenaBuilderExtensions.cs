using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Cyrena.Options;
using Cyrena.Services;
using Cyrena.Contracts;

namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        public static CyrenaBuilder AddComponents(this CyrenaBuilder builder)
        {
            var ui = new ComponentOptions();
            builder.AddFeatureOption(ui);

            builder.Services.AddBootstrapBlazor(options =>
            {
                options.DisableGetLocalizerFromResourceManager = true;
                options.DisableGetLocalizerFromService = true;
            });

            builder.AddAssistantPlugin<ComponentAssistantsPlugin>();

            builder.AddBuildAction(b =>
            {
                var uio = b.GetFeatureOption<ComponentOptions>();
                b.Services.AddSingleton(uio);
            });
            return builder;
        }

        public static CyrenaBuilder AddNavigationComponent<TComponent>(this CyrenaBuilder builder)
            where TComponent : ComponentBase
        {
            ComponentOptions ui = builder.GetFeatureOption<ComponentOptions>();
            ui.AddNavigationComponent<TComponent>();
            return builder;
        }

        public static CyrenaBuilder AddSettingsComponent<TComponent>(this CyrenaBuilder builder)
            where TComponent : ComponentBase
        {
            ComponentOptions ui = builder.GetFeatureOption<ComponentOptions>();
            ui.AddSettingsComponent<TComponent>();
            return builder;
        }

        public static CyrenaBuilder AddRouterAssembly<T>(this CyrenaBuilder builder)
        {
            ComponentOptions ui = builder.GetFeatureOption<ComponentOptions>();
            ui.AddRouterAssembly<T>();
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
