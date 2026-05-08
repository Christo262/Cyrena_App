using Cyrena.Coding.Contracts;
using Cyrena.Extensions;
using Cyrena.Options;
using Cyrena.Website.Models;
using Cyrena.Website.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.Website.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        public static CyrenaBuilder AddWebsite(this CyrenaBuilder builder)
        {
            builder.Services.AddSingleton<ICodeBuilder, WebsiteCodeBuilder>();
            builder.AddShortcut<WebsiteShortcut>();
            return builder;
        }
    }
}
