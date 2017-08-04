using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace SimpleEventSourcing.Utils
{
    public static class StateExtensions
    {
        private static readonly Dictionary<Type, PropertyInfo> cachedResultProperties = new Dictionary<Type, PropertyInfo>();

        public static TState ExtractState<TState>(this object result)
            where TState : class
        {
            if (result == null)
            {
                return null;
            }

            if (result is Task)
            {
                var type = result.GetType();

                if (!cachedResultProperties.TryGetValue(type, out PropertyInfo property))
                {
                    property = type.GetRuntimeProperty("Result");
                    cachedResultProperties[type] = property;
                }

                return property.GetValue(result) as TState;
            }

            return (TState)result;
        }
    }
}
