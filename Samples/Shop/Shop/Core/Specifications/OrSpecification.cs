namespace Shop.Core.Specifications
{
    public class OrSpecification<T> : Specification<T>
    {
        public OrSpecification(ISpecification<T> specification1, ISpecification<T> specification2)
        {
            Predicate = specification1.Predicate.Or(specification2.Predicate);
        }
    }
}
