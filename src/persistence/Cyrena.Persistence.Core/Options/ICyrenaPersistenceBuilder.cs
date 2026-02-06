using Cyrena.Models;

namespace Cyrena.Persistence.Options
{
    public interface ICyrenaPersistenceBuilder
    {
        void AddScopedStore<TEntity>(string collectionName) where TEntity : class, IEntity;
        void AddSingletonStore<TEntity>(string collectionName) where TEntity : class, IEntity;
    }
}
