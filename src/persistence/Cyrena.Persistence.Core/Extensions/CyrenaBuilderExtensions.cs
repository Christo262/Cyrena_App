using Cyrena.Models;
using Cyrena.Options;
using Cyrena.Persistence.Options;

namespace Cyrena.Extensions
{
    public static class CyrenaBuilderExtensions
    {
        public static CyrenaBuilder AddScopedStore<TEntity>(this CyrenaBuilder builder, string collectionName) where TEntity : class, IEntity
        {
            var p = builder.GetOption<ICyrenaPersistenceBuilder>();
            p.AddScopedStore<TEntity>(collectionName);
            return builder;
        }

        public static CyrenaBuilder AddSingletonStore<TEntity>(this CyrenaBuilder builder, string collectionName) where TEntity : class, IEntity
        {
            var p = builder.GetOption<ICyrenaPersistenceBuilder>();
            p.AddSingletonStore<TEntity>(collectionName);
            return builder;
        }
    }
}
