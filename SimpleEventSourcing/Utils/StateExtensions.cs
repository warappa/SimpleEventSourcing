using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace SimpleEventSourcing.Utils
{
    public static class StateExtensions
    {
        private static readonly Dictionary<Type, PropertyInfo> cachedResultProperties = new();

        public static TState ExtractState<TState>(this object result)
            where TState : class
        {
            if (result == null)
            {
                return null;
            }

            if(result is TState res)
            {
                return res;
            }

            var wasTask = false;

            if (result is Task task)
            {
                task.Wait();
                wasTask = true;
            }
            else if (result is ValueTask valueTask)
            {
                valueTask.GetAwaiter().GetResult();

                wasTask = true;
            }

            if (wasTask)
            {
                var type = result.GetType();

                if (!cachedResultProperties.TryGetValue(type, out var property))
                {
                    property = type.GetRuntimeProperty("Result");
                    if (property.PropertyType.Name == "VoidTaskResult")
                    {
                        property = null;
                    }
                    cachedResultProperties[type] = property;
                }

                result = property?.GetValue(result);
            }

            return (TState)result;
        }
    }
}
