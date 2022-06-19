using System;
using System.Linq.Expressions;

namespace Shop.Core.Specifications
{
    public interface ISpecification<T>
    {
        bool IsSatisfiedBy(T obj);
        Expression<Func<T, bool>> Predicate { get; }
    }
}
