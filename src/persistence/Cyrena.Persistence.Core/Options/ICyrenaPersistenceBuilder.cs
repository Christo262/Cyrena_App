using Cyrena.Models;

namespace Cyrena.Persistence.Options
{
    /// <summary>
    /// Helper object to easily add new entities for storage during DI builds
    /// </summary>
    public interface ICyrenaPersistenceBuilder
    {
        void AddScopedStore<TEntity>(string collectionName) where TEntity : class, IEntity;
        void AddSingletonStore<TEntity>(string collectionName) where TEntity : class, IEntity;
    }
}
