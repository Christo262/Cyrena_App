using Cyrena.Models;
using System.Linq.Expressions;

namespace Cyrena.Persistence.Specifications
{
    public class EntityIdSpecification<T> : Specification<T>
        where T : class, IEntity
    {
        private readonly string _id;
        public EntityIdSpecification(string id)
        {
            _id = id;
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            return x => x.Id == _id;
        }
    }
}
