using Cyrena.Models;
using System.Linq.Expressions;

namespace Cyrena.Persistence.Specifications
{
    public class AllSpecification<T> : Specification<T>
        where T : class, IEntity
    {
        public override Expression<Func<T, bool>> ToExpression()
        {
            return x => true;
        }
    }
}
