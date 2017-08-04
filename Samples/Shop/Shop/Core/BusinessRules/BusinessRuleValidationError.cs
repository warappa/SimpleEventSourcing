namespace Shop.Core.BusinessRules
{
    public class BusinessRuleValidationError
    {
        public string Message { get; set; }
        public string Key { get; set; }

        public BusinessRuleValidationError(string key, string message)
        {
            Message = message;
            Key = key;
        }
    }
}
