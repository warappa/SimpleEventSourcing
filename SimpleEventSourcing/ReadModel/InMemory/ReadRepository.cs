using SimpleEventSourcing.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace SimpleEventSourcing.ReadModel.InMemory
{
    public class ReadRepository : IReadRepository, IStorageResetter, IDbScopeAware
    {
        private List<string> existingTables = new();
        private readonly List<IReadModelBase> savedEntities = new();

        public bool IsTableInDatabase(Type type)
        {
            var name = type.Name;

            return existingTables.Contains(name) ||
                savedEntities.Any(x => x.GetType().Name == name);
        }

        public Task DeleteAsync(params IReadModelBase[] entities)
        {
            foreach (var entity in entities)
            {
                savedEntities.RemoveAll(x => x.Id == entity.Id);
            }

            return Task.CompletedTask;
        }

        public async Task<T> GetAsync<T>(object id)
            where T : class, IReadModelBase, new()
        {
            return (T)(await GetAsync(typeof(T), id).ConfigureAwait(false));
        }

        public Task<object> GetAsync(Type entityType, object id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var result = savedEntities
                .SingleOrDefault(x =>
                    entityType.GetTypeInfo().IsAssignableFrom(x.GetType().GetTypeInfo()) &&
                    id.Equals(x.Id));

            return Task.FromResult<object>(result);
        }

        public async Task<T> GetByStreamnameAsync<T>(object streamname)
             where T : class, IStreamReadModel, new()
        {
            return (T)(await GetByStreamnameAsync(typeof(T), streamname).ConfigureAwait(false));
        }

        public Task<object> GetByStreamnameAsync(Type entityType, object streamname)
        {
            var result = savedEntities
               .OfType<IStreamReadModel>()
               .SingleOrDefault(x =>
                   entityType.GetTypeInfo().IsAssignableFrom(x.GetType().GetTypeInfo()) &&
                   x.Streamname?.Equals(streamname) == true);

            return Task.FromResult<object>(result);
        }

        public async Task InsertAsync(params IReadModelBase[] entities)
        {
            foreach (var entity in entities.ToList())
            {
                if (await GetAsync(entity.GetType(), entity.Id).ConfigureAwait(false) != null)
                {
                    throw new InvalidOperationException("Enity already exists!");
                }

                savedEntities.Add(entity);
            }
        }

        public async Task<IQueryable<T>> QueryAsync<T>(Expression<Func<T, bool>> predicate)
            where T : class, IReadModelBase, new()
        {
            return (await QueryAsync(typeof(T), x => predicate.Compile()((T)x)).ConfigureAwait(false))
                .AsQueryable()
                .Cast<T>();
        }

        public Task<IQueryable> QueryAsync(Type type, Expression<Func<object, bool>> predicate)
        {
            var result = savedEntities
                .Where(x => type.GetTypeInfo().IsAssignableFrom(x.GetType().GetTypeInfo()))
                .Cast<object>()
                .Where(predicate.Compile())
                .AsQueryable();

            return Task.FromResult<IQueryable>(result);
        }

        public async Task UpdateAsync(params IReadModelBase[] entities)
        {
            await DeleteAsync(entities).ConfigureAwait(false);

            foreach (var entity in entities)
            {
                savedEntities.Add(entity);
            }
        }

        public async Task ResetAsync(Type[] entityTypes, bool justDrop = false)
        {
            var entityNames = entityTypes.Select(x => x.Name).ToList();

            foreach (var entity in savedEntities.Where(x => entityNames.Contains(x.GetType().Name)).ToList())
            {
                savedEntities.Remove(entity);
            }

            if (justDrop)
            {
                existingTables = existingTables.Except(entityNames).ToList();
            }
            else
            {
                existingTables = existingTables.Union(entityNames).ToList();
            }
        }

        public IDisposable OpenScope()
        {
            return new NullDisposable();
        }
    }
}
