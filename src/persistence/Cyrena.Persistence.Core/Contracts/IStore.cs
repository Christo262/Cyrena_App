using Cyrena.Models;

namespace Cyrena.Persistence.Contracts
{
    public interface IStore<T> : IDisposable
            where T : class, IEntity
    {
        IQueryable<T> QueryableData { get; }
        Task SaveAsync(T entity, CancellationToken cancellationToken = default);
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        Task AddManyAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
        Task<int> DeleteManyAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> FindManyAsync(ISpecification<T> specification, IOrderBy<T>? orderBy = default, IPaging? paging = default, CancellationToken cancellationToken = default);
        Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
        Task<T?> FindAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
    }
}
