using System.Reflection;
using Cyrena.Attributes;
using Cyrena.Contracts;
using Cyrena.Models;
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
                var models = b.BuildViewStartComponents();
                var srv = new AttributeViewStartProvider(models);
                b.Services.AddSingleton<IViewStartProvider>(srv);
            });
            return builder;
        }

        private static List<ViewStart> BuildViewStartComponents(this CyrenaBuilder builder)
        {
            var assemblies = builder.FeatureAssemblies.ContainsKey("blazor") ? builder.FeatureAssemblies["blazor"] : Enumerable.Empty<Assembly>();
            var models = new List<ViewStart>();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes().Where(x => x.GetCustomAttribute<ViewStartAttribute>() != null);
                foreach (var type in types)
                {
                    var attribute = type.GetCustomAttribute<ViewStartAttribute>()!;
                    var model = new ViewStart(attribute.Id, type, attribute.Title, attribute.Description);
                    models.Add(model);
                }
            }
            return models;
        }
    }
}
