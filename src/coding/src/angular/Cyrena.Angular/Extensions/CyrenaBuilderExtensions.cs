using Cyrena.Angular.Models;
using Cyrena.Angular.Services;
using Cyrena.Coding.Contracts;
using Cyrena.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        public static CyrenaBuilder AddAngular(this CyrenaBuilder builder)
        {
            builder.Services.AddSingleton<ICodeBuilder, AngularBuilder>();
            builder.AddShortcut<AngularShortcut>();
            return builder;
        }
    }
}
