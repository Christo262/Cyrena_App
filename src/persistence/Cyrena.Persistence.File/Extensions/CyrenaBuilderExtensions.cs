using Microsoft.Extensions.DependencyInjection;
using Cyrena.Contracts;
using Cyrena.Options;
using Cyrena.Persistence.File.Options;
using Cyrena.Persistence.Options;
using Cyrena.Services;

namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        public static CyrenaBuilder UseFilePersistence(this CyrenaBuilder builder, Action<FilePersistenceOptions> options)
        {
            var p = new FilePersistenceBuilder(builder.Services);
            builder.AddFeatureOption<ICyrenaPersistenceBuilder>(p);
            builder.Services.AddSingleton<IPersistenceFS, PersistenceFS>();
            builder.Services.Configure(options);
            return builder;
        }
    }
}
