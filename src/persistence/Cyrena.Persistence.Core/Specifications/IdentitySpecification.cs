using System.Linq.Expressions;

namespace Cyrena.Persistence
{
    internal struct IdentitySpecification<T> : ISpecification<T>
    {
        public bool IsSatisfiedBy(T entity) => true;
        public Expression<Func<T, bool>> ToExpression() => x => true;
    }
}
