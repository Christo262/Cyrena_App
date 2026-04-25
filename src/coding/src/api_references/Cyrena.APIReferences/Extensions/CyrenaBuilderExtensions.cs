using Cyrena.APIReferences.Models;
using Cyrena.APIReferences.Services;
using Cyrena.Options;

namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        public static CyrenaBuilder AddApiReferencePages(this CyrenaBuilder builder)
        {
            builder.AddFeatureAssembly<ApiReference>("blazor");
            builder.AddAssistantPlugin<AssistantPlugin>();
            return builder;
        }
    }
}
