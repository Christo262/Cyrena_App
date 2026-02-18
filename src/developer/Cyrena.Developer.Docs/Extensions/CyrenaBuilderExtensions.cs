using Cyrena.Developer.Docs.Models;
using Cyrena.Options;

namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        public static CyrenaBuilder AddApiReferencePages(this CyrenaBuilder builder)
        {
            builder.AddFeatureAssembly<ApiReference>("blazor");
            return builder;
        }
    }
}
