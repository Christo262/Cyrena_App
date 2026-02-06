using Cyrena.Contracts;
using Cyrena.Options;
using Cyrena.Spec.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        public static CyrenaBuilder AddSpecifications(this CyrenaBuilder builder)
        {
            builder.Services.AddScoped<IDeveloperContextExtension, SpecsExtension>();
            return builder;
        }
    }
}
