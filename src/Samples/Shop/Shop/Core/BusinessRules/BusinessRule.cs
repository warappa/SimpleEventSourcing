using Shop.Core.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shop.Core.BusinessRules
{
    public class BusinessRule<T> : IBusinessRule
    {
        public string Name { get; protected set; }

        protected IDictionary<string, CheckItem> checks = new Dictionary<string, CheckItem>();
        protected IDictionary<string, CheckItem> unsuccessfulChecks = new Dictionary<string, CheckItem>();

        public BusinessRule(string name)
        {
            Name = name;
        }

        public BusinessRule(string name, Action<BusinessRule<T>> ruleConfig)
            : this(name)
        {
            ruleConfig(this);
        }

        protected class CheckItem
        {
            public string Description { get; set; }
            public string PropertyName { get; set; }
            public Func<T, bool> Check { get; set; }

            public CheckItem() { }

            public CheckItem(string description, Func<T, bool> check, string propertyName = null)
            {
                Description = description;
                Check = check;
                PropertyName = propertyName;
            }
        }

        public BusinessRule<T> AddCheck(Func<T, bool> isSatisfied, string description)
        {
            checks.Add(description, new CheckItem(description, isSatisfied));

            return this;
        }

        public BusinessRule<T> AddCheck(string propertyName, Func<T, bool> isSatisfied, string description)
        {
            checks.Add(description, new CheckItem(description, isSatisfied, propertyName));

            return this;
        }

        public BusinessRule<T> AddCheck(ISpecification<T> specification, string description)
        {
            checks.Add(description, new CheckItem(description, specification.Predicate.Compile()));

            return this;
        }

        public BusinessRule<T> RemoveCheck(string description)
        {
            checks.Remove(description);

            return this;
        }

        public void Check(T obj)
        {
            if (!IsSatisfiedBy(obj))
            {
                throw new BusinessRuleException(Name, unsuccessfulChecks.Select(x => new BusinessRuleValidationError(x.Value.PropertyName ?? "", x.Value.Description)));
            }
        }

        public bool IsSatisfiedBy(T obj)
        {
            if (checks.Count == 0)
            {
                throw new InvalidOperationException("BusinessRule has no checks!");
            }

            unsuccessfulChecks.Clear();

            foreach (var check in checks)
            {
                if (check.Value.Check.Invoke(obj) == false)
                {
                    // add validation message
                    unsuccessfulChecks.Add(check);
                }
            }

            return unsuccessfulChecks.Count == 0;
        }

        void IBusinessRule.Check(object obj)
        {
            Check((T)obj);
        }

        bool IBusinessRule.IsSatisfiedBy(object obj)
        {
            return IsSatisfiedBy((T)obj);
        }
    }
}
