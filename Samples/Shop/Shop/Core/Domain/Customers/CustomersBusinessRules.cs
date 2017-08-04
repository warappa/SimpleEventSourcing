using Shop.Core.BusinessRules;

namespace Shop.Core.Domain.Customers
{
    public static class CustomersBusinessRules
    {
        public static BusinessRule<Customer> CanRename(string newName)
        {
            return new BusinessRule<Customer>(
                "Cannot rename customer!",
                rule =>
                {
                    rule.AddCheck(CustomersSpecifications.IsActive, "Customer is deactivated!");
                    rule.AddCheck(_ => !string.IsNullOrWhiteSpace(newName), "Name is empty!");
                });
        }

        public static BusinessRule<Customer> CanDeactivate()
        {
            return new BusinessRule<Customer>(
                "Cannot deactivate customer!",
                rule =>
                {
                    rule.AddCheck(CustomersSpecifications.IsActive, "Customer is already deactivated!");
                });
        }
    }
}
