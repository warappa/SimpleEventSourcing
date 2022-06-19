using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SimpleEventSourcing.EntityFrameworkCore.ReadModel
{
    public class DbSetInternal
    {
        private readonly object dbSet;
        private readonly Type entityType;

        public DbSetInternal(object dbSet, Type entityType)
        {
            this.dbSet = dbSet;
            this.entityType = entityType;
        }

        public void AddRange<T>(IEnumerable<T> entities)
            where T : class
        {
            var entitiesArray = ConvertToEntityArray(entities);

            var addRangeMi = dbSet.GetType().GetMethods().First(x => x.Name == "AddRange");
            addRangeMi.Invoke(dbSet, new[] { entitiesArray });
        }

        public void RemoveRange<T>(IEnumerable<T> entities)
            where T : class
        {
            var entitiesArray = ConvertToEntityArray(entities);
            var removeRangeMi = dbSet.GetType().GetMethods().First(x => x.Name == "RemoveRange");
            removeRangeMi.Invoke(dbSet, new[] { entitiesArray });
        }

        public virtual void Attach<T>(T entity)
            where T : class
        {
            var attachMi = dbSet.GetType().GetMethods().First(x => x.Name == "Attach");
            var result = attachMi.Invoke(dbSet, new object[] { entity });
        }

        public object Find(object id)
        {
            return InternalDbSetInterop.Find<object>(dbSet, id);
        }

        public IEnumerable Local
        {
            get
            {
                return InternalDbSetInterop.GetLocal(dbSet);
            }
        }

        private Array ConvertToEntityArray<T>(IEnumerable<T> entities) where T : class
        {
            var objectArray = entities.Cast<object>().ToArray();
            var length = objectArray.Length;
            var entitiesArray = Array.CreateInstance(entityType, length);
            for (var i = 0; i < length; i++)
            {
                entitiesArray.SetValue(objectArray[i], i);
            }

            return entitiesArray;
        }
    }
}
