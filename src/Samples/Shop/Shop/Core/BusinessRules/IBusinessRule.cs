namespace Shop.Core.BusinessRules
{
    public interface IBusinessRule
    {
        void Check(object obj);
        bool IsSatisfiedBy(object obj);
    }
}
