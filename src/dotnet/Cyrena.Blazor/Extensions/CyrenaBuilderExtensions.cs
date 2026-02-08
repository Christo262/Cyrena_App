using Microsoft.Extensions.DependencyInjection;
using Cyrena.Blazor.Services;
using Cyrena.Contracts;
using Cyrena.Options;

namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        public static CyrenaBuilder AddBlazorDevelopment(this CyrenaBuilder builder)
        {
            builder.Services.AddScoped<IProjectConfigurator, BlazorProjectConfigurator>();
            builder.Services.AddScoped<IProjectConfigurator, BlazorLibraryConfigurator>();
            return builder;
        }
    }
}
