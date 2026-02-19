using Cyrena.ArduinoIDE.Models;
using Cyrena.ArduinoIDE.Services;
using Cyrena.Contracts;
using Cyrena.Developer.Contracts;
using Cyrena.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        public static CyrenaBuilder AddArduinoIDE(this CyrenaBuilder builder)
        {
            builder.Services.AddSingleton<ICodeBuilder, ArduinoIDECodeBuilder>();
            builder.AddShortcut<ArduinoShortcut>();
            return builder;
        }
    }
}
