using System;
using System.Collections;
using System.Linq;

namespace SimpleEventSourcing.Tests.Storage
{
    public static class ListHelper
    {
        public static IList ToList(this IQueryable query)
        {
            var genericToList = typeof(Enumerable).GetMethod("ToList")
                .MakeGenericMethod(new Type[] { query.ElementType });
            return (IList)genericToList.Invoke(null, new[] { query });
        }
    }
}
