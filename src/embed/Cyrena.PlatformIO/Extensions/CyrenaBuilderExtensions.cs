using Cyrena.Contracts;
using Cyrena.Developer.Contracts;
using Cyrena.Options;
using Cyrena.PlatformIO.Models;
using Cyrena.PlatformIO.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        public static CyrenaBuilder AddPlatformIO(this CyrenaBuilder builder)
        {
            builder.Services.AddSingleton<ICodeBuilder, PlatformIOBuilder>();
            builder.AddShortcut<PlatformIOShortcut>();
            return builder;
        }
    }
}
