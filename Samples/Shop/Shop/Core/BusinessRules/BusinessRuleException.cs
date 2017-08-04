using System.Collections.Generic;
using System.Linq;

namespace Shop.Core.BusinessRules
{
    public class BusinessRuleException : BusinessException
    {
        public string Name { get; protected set; }
        public IEnumerable<BusinessRuleValidationError> Errors { get { return errors.ToArray(); } }

        protected IEnumerable<BusinessRuleValidationError> errors;

        public BusinessRuleException(string name, IEnumerable<BusinessRuleValidationError> errors = null)
            : base(GetMessage(name, errors))
        {
            Name = name;
            this.errors = errors ?? Enumerable.Empty<BusinessRuleValidationError>();
        }

        protected static string GetMessage(string name, IEnumerable<BusinessRuleValidationError> errors)
        {
            if (errors == null)
            {
                errors = Enumerable.Empty<BusinessRuleValidationError>();
            }

            return "Business rules were violated: " + name + "\n - " + string.Join("\n - ", errors.Select(x => (x.Key != "" ? x.Key + ": " : "") + x.Message));
        }

        public override string ToString()
        {
            return GetMessage(Name, Errors);
        }
    }
}
