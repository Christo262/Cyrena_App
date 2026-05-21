using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Cyrena.Contracts;
using Cyrena.Options;
using Cyrena.Persistence.Contracts;
using Cyrena.Persistence.File.Services;
using Cyrena.Persistence.Options;
using Cyrena.Services;

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
                return new FileStore<TEntity>(fs, collectionName);
            });
        }

        void ICyrenaPersistenceBuilder.AddSingletonStore<TEntity>(string collectionName)
        {
            _services.AddSingleton<IStore<TEntity>>(sp =>
            {
                var fs = sp.GetRequiredService<IPersistenceFS>();
                return new FileStore<TEntity>(fs, collectionName);
            });
        }
    }

    internal class IsolatedFilePersistenceBuilder : ICyrenaPersistenceBuilder
    {
        private readonly IServiceCollection _services;
        private readonly PersistenceFS _fs;
        public IsolatedFilePersistenceBuilder(IServiceCollection services, FilePersistenceOptions options)
        {
            _services = services;
            _fs = new PersistenceFS(options);
        }

        void ICyrenaPersistenceBuilder.AddScopedStore<TEntity>(string collectionName)
        {
            _services.AddScoped<IStore<TEntity>>(sp =>
            {
                return new FileStore<TEntity>(_fs, collectionName);
            });
        }

        void ICyrenaPersistenceBuilder.AddSingletonStore<TEntity>(string collectionName)
        {
            _services.AddSingleton<IStore<TEntity>>(sp =>
            {
                return new FileStore<TEntity>(_fs, collectionName);
            });
        }
    }
}
