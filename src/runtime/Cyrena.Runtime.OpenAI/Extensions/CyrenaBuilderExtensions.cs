using Cyrena.Contracts;
using Cyrena.Options;
using Cyrena.Runtime.OpenAI.Components.Shared;
using Cyrena.Runtime.OpenAI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        public static CyrenaBuilder AddOpenAI(this CyrenaBuilder builder)
        {
            builder.Services.AddScoped<IConnectionProvider, ConnectionProvider>();
            builder.AddSettingsComponent<OpenAISettings>();
            return builder;
        }
    }
}
