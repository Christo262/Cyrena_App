using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Cyrena.Contracts;
using Cyrena.Options;
using Cyrena.Persistence.Contracts;
using Cyrena.Persistence.File.Services;
using Cyrena.Persistence.Options;

namespace Cyrena.Persistence.File.Options
{
    internal class FilePersistenceBuilder : ICyrenaPersistenceBuilder
    {
        private readonly IServiceCollection _services;
        public FilePersistenceBuilder(IServiceCollection services)
        {
            _services = services;
        }

        void ICyrenaPersistenceBuilder.AddScopedStore<TEntity>(string collectionName)
        {
            _services.AddScoped<IStore<TEntity>>(sp =>
            {
                var fs = sp.GetRequiredService<IPersistenceFS>();
                var op = sp.GetRequiredService<IOptions<FilePersistenceOptions>>();
                return new FileStore<TEntity>(op, fs, collectionName);
            });
        }

        void ICyrenaPersistenceBuilder.AddSingletonStore<TEntity>(string collectionName)
        {
            _services.AddSingleton<IStore<TEntity>>(sp =>
            {
                var fs = sp.GetRequiredService<IPersistenceFS>();
                var op = sp.GetRequiredService<IOptions<FilePersistenceOptions>>();
                return new FileStore<TEntity>(op, fs, collectionName);
            });
        }
    }
}
