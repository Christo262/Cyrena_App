using Cyrena.Contracts;
using Cyrena.Options;
using Cyrena.PlatformIO.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        public static CyrenaBuilder AddPlatformIO(this CyrenaBuilder builder)
        {
            builder.Services.AddScoped<IProjectConfigurator, PlatformIOConfigurator>();
            return builder;
        }
    }
}
