using Microsoft.Extensions.DependencyInjection;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Options;
using Cyrena.Tavily.Components.Shared;
using Cyrena.Tavily.Services;

namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        public static CyrenaBuilder AddTavily(this CyrenaBuilder builder)
        {
            builder.AddAssistantPlugin<TavilyExtension>();
            builder.AddSettingsComponent<TavilySettings>();

            return builder;
        }
    }
}
