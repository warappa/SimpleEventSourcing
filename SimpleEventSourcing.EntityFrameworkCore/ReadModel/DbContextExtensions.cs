using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleEventSourcing.EntityFrameworkCore.ReadModel
{
    public static class DbContextExtensions
    {
        private static readonly MethodInfo setMethodInfo;
        private static readonly ConcurrentDictionary<Type, MethodInfo> setMethods;
        private static readonly ConcurrentDictionary<Type, Type> iSetClasses;

        static DbContextExtensions()
        {
            setMethodInfo = typeof(DbContext).GetMethods()
                .Where(x => x.Name == nameof(DbContext.Set))
                .First();
            setMethods = new();
            iSetClasses = new();
        }

        public static DbSetInternal Set(this DbContext dbContext, Type type)
        {
            var value = setMethods.GetOrAdd(type, t => setMethodInfo.MakeGenericMethod(t));
            var valueInterface = setMethods.GetOrAdd(type, t => setMethodInfo.MakeGenericMethod(t));
            var set = value.Invoke(dbContext, Array.Empty<object>());

            return new DbSetInternal(set, type);
        }
    }
}
