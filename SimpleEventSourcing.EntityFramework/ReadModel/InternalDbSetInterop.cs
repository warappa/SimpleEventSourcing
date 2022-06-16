using System;
using System.Collections.Concurrent;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace SimpleEventSourcing.EntityFramework.ReadModel
{
    internal static class InternalDbSetInterop
    {
        private static ConcurrentDictionary<Type, MethodInfo> findMethodInfos = new ConcurrentDictionary<Type, MethodInfo>();
        private static ConcurrentDictionary<Type, PropertyInfo> localPropertyInfos = new ConcurrentDictionary<Type, PropertyInfo>();

        public static T Find<T>(DbSet set, object id)
        {
            if (!id.GetType().IsArray)
            {
                return (T)set.Find(id);
            }
            else
            {
                var findMi = InternalDbSetInterop.GetFindMethod(set);
                if (id.GetType().GetElementType() == typeof(object))
                {
                    return (T)findMi.Invoke(set, new object[] { id });
                }

                return (T)findMi.Invoke(set, new object[] { ((Array)id).Cast<object>().ToArray() });
            }
        }

        public static MethodInfo GetFindMethod(object internalDbSet)
        {
            if (!findMethodInfos.TryGetValue(internalDbSet.GetType(), out var findMi))
            {
                findMi ??= internalDbSet.GetType().GetMethod("Find", new[] { typeof(object[]) });
                findMethodInfos[internalDbSet.GetType()] = findMi;
            }

            return findMi;
        }

        public static PropertyInfo GetLocalProperty(object dbSet)
        {
            if (!localPropertyInfos.TryGetValue(dbSet.GetType(), out var localPi))
{
                localPi = dbSet.GetType().GetProperty("Local");
                localPropertyInfos[dbSet.GetType()] = localPi;
            }

            return localPi;
        }
    }
}
