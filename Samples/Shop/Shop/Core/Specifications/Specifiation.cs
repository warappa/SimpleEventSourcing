using System;
using System.Linq.Expressions;

namespace Shop.Core.Specifications
{
    public class Specification<T> : ISpecification<T>
    {
        protected Func<T, bool> compiledExpression;

        public Expression<Func<T, bool>> Predicate { get; protected set; }

        public Specification()
        {
            Predicate = x => true;
        }

        public Specification(Expression<Func<T, bool>> expression)
        {
            Predicate = expression;
        }

        public Specification<T> Config(Action<Specification<T>> config)
        {
            config(this);

            return this;
        }

        public virtual bool IsSatisfiedBy(T obj)
        {
            return (compiledExpression ?? (compiledExpression = Predicate.Compile()))
                .Invoke(obj);
        }
    }
}
