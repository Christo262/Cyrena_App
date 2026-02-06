using Cyrena.ClassLibrary.Services;
using Cyrena.Contracts;
using Cyrena.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        public static CyrenaBuilder AddClassLibraryDevelopment(this CyrenaBuilder builder)
        {
            builder.Services.AddScoped<IProjectConfigurator, LibraryConfigurator>();
            return builder;
        }
    }
}
