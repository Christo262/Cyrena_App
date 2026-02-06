using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Cyrena.Options;

namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        public static CyrenaBuilder AddComponents(this CyrenaBuilder builder)
        {
            var ui = new ComponentOptions();
            builder.AddOption("Cyrena.components", ui);

            builder.Services.AddBootstrapBlazor();

            builder.AddBuildAction(b =>
            {
                var uio = b.GetOption<ComponentOptions>();
                b.Services.AddSingleton(uio);
            });
            return builder;
        }

        public static CyrenaBuilder AddNavigationComponent<TComponent>(this CyrenaBuilder builder)
            where TComponent : ComponentBase
        {
            ComponentOptions ui = builder.GetOption<ComponentOptions>();
            ui.AddNavigationComponent<TComponent>();
            return builder;
        }

        public static CyrenaBuilder AddSettingsComponent<TComponent>(this CyrenaBuilder builder)
            where TComponent : ComponentBase
        {
            ComponentOptions ui = builder.GetOption<ComponentOptions>();
            ui.AddSettingsComponent<TComponent>();
            return builder;
        }

        public static CyrenaBuilder AddRouterAssembly<T>(this CyrenaBuilder builder)
        {
            ComponentOptions ui = builder.GetOption<ComponentOptions>();
            ui.AddRouterAssembly<T>();
            return builder;
        }
    }
}
