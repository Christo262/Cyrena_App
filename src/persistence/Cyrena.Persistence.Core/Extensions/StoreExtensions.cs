using Cyrena.Models;
using Cyrena.Persistence;
using Cyrena.Persistence.Contracts;
using System.Linq.Expressions;

namespace Cyrena.Extensions
{
    public static class StoreExtensions
    {
        public static Task<T?> FindAsync<T>(this IStore<T> store, Expression<Func<T, bool>> predicate, CancellationToken ct = default)
           where T : class, IEntity
        {
            return store.FindAsync(new AnySpecification<T>(predicate), ct);
        }

        public static Task<IEnumerable<T>> FindManyAsync<T>(this IStore<T> store, Expression<Func<T, bool>> predicate, OrderBy<T>? orderBy = null, Paging? paging = null, CancellationToken ct = default)
            where T : class, IEntity
        {
            return store.FindManyAsync(new AnySpecification<T>(predicate), orderBy, paging, ct);
        }

        public static Task<int> CountAsync<T>(this IStore<T> store, Expression<Func<T, bool>> predicate, CancellationToken ct = default) where T : class, IEntity
        {
            return store.CountAsync(new AnySpecification<T>(predicate), ct);
        }

        public static Task<int> DeleteManyAsync<T>(this IStore<T> store, Expression<Func<T, bool>> predicate, CancellationToken ct = default) where T : class, IEntity
        {
            return store.DeleteManyAsync(new AnySpecification<T>(predicate), ct);
        }
    }

    internal class AnySpecification<T> : Specification<T>
    {
        private readonly Expression<Func<T, bool>> _predicate;
        public AnySpecification(Expression<Func<T, bool>> expression)
        {
            _predicate = expression;
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            return _predicate;
        }
    }
}
