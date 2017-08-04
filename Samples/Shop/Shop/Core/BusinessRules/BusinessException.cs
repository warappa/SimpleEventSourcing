using System;

namespace Shop.Core.BusinessRules
{
    public class BusinessException : Exception
    {
        public BusinessException()
        {
        }

        public BusinessException(string message)
            : base(message)
        {
        }

        public BusinessException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected BusinessException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }

        public override string ToString()
        {
            return "BusinessException: " + Message;
        }
    }
}
