using Cyrena.Contracts;
using Cyrena.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        public static CyrenaBuilder AddSettingsComponent<TComponent>(this CyrenaBuilder builder)
            where TComponent : ComponentBase
        {
            ComponentOptions ui = builder.GetFeatureOption<ComponentOptions>();
            ui.AddSettingsComponent<TComponent>();
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
