using Cyrena.ArduinoIDE.Services;
using Cyrena.Contracts;
using Cyrena.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        public static CyrenaBuilder AddArduinoIDE(this CyrenaBuilder builder)
        {
            builder.Services.AddScoped<IProjectConfigurator, ArduinoProjectConfigurator>();
            return builder;
        }
    }
}
