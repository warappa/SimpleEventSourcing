namespace Shop.Core.Specifications
{
    public class AndSpecification<T> : Specification<T>
    {
        public AndSpecification(ISpecification<T> specification1, ISpecification<T> specification2)
        {
            Predicate = specification1.Predicate.And(specification2.Predicate);
        }
    }
}
