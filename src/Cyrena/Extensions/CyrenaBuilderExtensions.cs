using Cyrena.Options;
using Cyrena.Services;

namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        public static CyrenaBuilder AddDefaultAssistant(this CyrenaBuilder builder)
        {
            builder.AddAssistantMode<DefaultAssistantMode>();
            return builder;
        }
    }
}
