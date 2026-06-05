using Cyrena.Components.Shared;
using Cyrena.Contracts;
using Cyrena.Options;
using Cyrena.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        public static CyrenaBuilder AddDefaultAssistant(this CyrenaBuilder builder)
        {
            builder.AddAssistantMode<DefaultAssistantMode>();
            builder.AddFeatureAssembly<DefaultAssistantMode>("blazor");
            builder.AddSettingsComponent<CustomizationSettings>("Personalization");
            builder.Services.AddScoped<HeadOutletStateChangeTracker>();
            builder.Services.AddSingleton<IViewStartProvider, DefaultViewStartProvider>();
            builder.Services.AddScoped<IViewStart, ViewStartService>();
            builder.AddAssistantPlugin<ToolCallInclusionPlugin>();
            builder.AddSettingsComponent<Settings>("Application", 0);
            return builder;
        }
    }
}
