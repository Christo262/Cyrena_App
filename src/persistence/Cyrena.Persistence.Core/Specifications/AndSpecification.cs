using LinqKit;
using System.Linq.Expressions;

namespace Cyrena.Persistence
{
    public struct AndSpecification<T> : ISpecification<T>
    {
        public AndSpecification(ISpecification<T> left, ISpecification<T> right)
        {
            Right = right;
            Left = left;
        }

        public bool IsSatisfiedBy(T entity) => Left.IsSatisfiedBy(entity) && Right.IsSatisfiedBy(entity);
        public Expression<Func<T, bool>> ToExpression() => Left.ToExpression().And(Right.ToExpression());

        public ISpecification<T> Left { get; }
        public ISpecification<T> Right { get; }
    }
}
