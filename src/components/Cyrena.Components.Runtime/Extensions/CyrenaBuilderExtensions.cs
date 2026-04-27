using Microsoft.Extensions.DependencyInjection;
using Cyrena.Options;
using Cyrena.Services;

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
                
            }).ConfigureIconThemeOptions(icons =>
            {
                icons.ThemeKey = "bootstrap";
            });

            builder.AddAssistantPlugin<ComponentAssistantsPlugin>();

            builder.AddBuildAction(b =>
            {
                var uio = b.GetFeatureOption<ComponentOptions>();
                b.Services.AddSingleton(uio);
            });
            return builder;
        }
    }
}
