using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace SimpleEventSourcing.EntityFrameworkCore.ReadModel
{
    internal static class InternalDbSetInterop
    {
        private static ConcurrentDictionary<Type, MethodInfo> findMethodInfos = new ConcurrentDictionary<Type, MethodInfo>();
        private static ConcurrentDictionary<Type, PropertyInfo> localPropertyInfos = new ConcurrentDictionary<Type, PropertyInfo>();

        public static T Find<T>(object set, object id)
        {
            var findMi = InternalDbSetInterop.GetFindMethod(set);
            var idType = id.GetType();
            if (!idType.IsArray)
            {
                return (T)findMi.Invoke(set, new object[] { new object[] { id } });
            }
            else
            {
                if (idType.GetElementType() == typeof(object))
                {
                    return (T)findMi.Invoke(set, new object[] { id });
                }

                return (T)findMi.Invoke(set, new object[] { ((Array)id).Cast<object>().ToArray() });
            }
        }

        private static MethodInfo GetFindMethod(object internalDbSet)
        {
            var dbSetType = internalDbSet.GetType();
            if (!findMethodInfos.TryGetValue(dbSetType, out var findMi))
            {
                findMi ??= dbSetType.GetMethod("Find", new[] { typeof(object[]) });
                findMethodInfos[dbSetType] = findMi;
            }

            return findMi;
        }

        public static IEnumerable GetLocal(object dbSet)
        {
            var localPi = InternalDbSetInterop.GetLocalProperty(dbSet);
            return (IEnumerable)localPi.GetValue(dbSet);
        }

        private static PropertyInfo GetLocalProperty(object dbSet)
        {
            var dbSetType = dbSet.GetType();
            if (!localPropertyInfos.TryGetValue(dbSetType, out var localPi))
            {
                localPi = dbSetType.GetProperty("Local");
                localPropertyInfos[dbSetType] = localPi;
            }

            return localPi;
        }
    }
}
