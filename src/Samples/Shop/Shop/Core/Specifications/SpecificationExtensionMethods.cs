namespace Shop.Core.Specifications
{
    public static class SpecificationExtensionMethods
    {
        public static ISpecification<T> And<T>(this ISpecification<T> specification1, ISpecification<T> specification2)
        {
            return new AndSpecification<T>(specification1, specification2);
        }

        public static ISpecification<T> Or<T>(this ISpecification<T> specification1, ISpecification<T> specification2)
        {
            return new OrSpecification<T>(specification1, specification2);
        }

        public static ISpecification<T> Not<T>(this ISpecification<T> specification)
        {
            return new NotSpecification<T>(specification);
        }
    }
}
