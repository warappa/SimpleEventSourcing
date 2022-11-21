using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleEventSourcing.Tests.WriteModel
{
    public static class ReflectionExtensions
    {
        public static IEnumerable<Type> GetGenericInterfaces(this Type type, Type genericTypeDefinition)
        {
            return type.GetInterfaces().Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == genericTypeDefinition);
        }
    }
}
