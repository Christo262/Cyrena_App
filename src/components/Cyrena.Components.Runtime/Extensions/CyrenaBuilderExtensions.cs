using Cyrena.Contracts;
using Cyrena.Options;
using Cyrena.Services;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        public static CyrenaBuilder AddComponents(this CyrenaBuilder builder)
        {
            var ui = new ComponentOptions();
            builder.AddFeatureOption(ui);

            builder.Services.AddMudServices();

            builder.AddAssistantPlugin<ComponentAssistantsPlugin>();

            builder.AddBuildAction(b =>
            {
                var uio = b.GetFeatureOption<ComponentOptions>();
                b.Services.AddSingleton(uio);
            });
            builder.Services.AddScoped<IViewStartProvider, ViewStartProvider>();
            return builder;
        }
    }
}
