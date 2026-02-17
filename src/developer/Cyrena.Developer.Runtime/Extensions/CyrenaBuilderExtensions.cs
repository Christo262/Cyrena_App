using Cyrena.Developer.Services;
using Cyrena.Extensions;
using Cyrena.Options;

namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        public static CyrenaBuilder AddDeveloperRuntime(this CyrenaBuilder builder)
        {
            builder.AddAssistantMode<DeveloperAssistantMode>();
            builder.AddFeatureAssembly<DeveloperAssistantMode>("blazor");
            return builder;
        }
    }
}
