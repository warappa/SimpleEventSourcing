using System;
using System.Linq;
using System.Linq.Expressions;

namespace Shop.Core.Specifications
{
    public class NotSpecification<T> : Specification<T>
    {
        public NotSpecification(ISpecification<T> specification)
        {
            var predicate = specification.Predicate;

            Predicate = Expression.Lambda<Func<T, bool>>(
                Expression.Not(predicate.Body),
                predicate.Parameters.Single()
            );
        }
    }
}
