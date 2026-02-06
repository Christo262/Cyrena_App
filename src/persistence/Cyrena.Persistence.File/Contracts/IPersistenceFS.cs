using Cyrena.Models;

namespace Cyrena.Contracts
{
    public interface IPersistenceFS
    {
        IList<T> Read<T>(string collectionName)
            where T : class, IEntity;
        void Write<T>(T entity, string collectionName)
            where T : class, IEntity;
        void Delete<T>(string id, string collectionName)
            where T : class, IEntity;
    }
}
