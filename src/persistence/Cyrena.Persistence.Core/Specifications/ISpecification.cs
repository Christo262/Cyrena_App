using System.Linq.Expressions;

namespace Cyrena.Persistence
{
    public interface ISpecification<T>
    {
        bool IsSatisfiedBy(T entity);
        Expression<Func<T, bool>> ToExpression();
    }
}
