﻿using EntityFrameworkCore.DbContextScope;
using Microsoft.EntityFrameworkCore;
using SimpleEventSourcing.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace SimpleEventSourcing.EntityFrameworkCore.ReadModel
{
    public partial class ReadRepository<TDbContext> : IReadRepository, IDbScopeAware
        where TDbContext : DbContext
    {
        private readonly IDbContextScopeFactory dbContextScopeFactory;

        public ReadRepository(IDbContextScopeFactory dbContextScopeFactory)
        {
            this.dbContextScopeFactory = dbContextScopeFactory;
        }

        public async Task UpdateAsync(params IReadModelBase[] entities)
        {
            using var scope = dbContextScopeFactory.Create();
            var dbContext = scope.DbContexts.Get<TDbContext>();

            UpdateInternal(dbContext, entities);

            await scope.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task InsertAsync(params IReadModelBase[] entities)
        {
            using var scope = dbContextScopeFactory.Create();
            var dbContext = scope.DbContexts.Get<TDbContext>();

            InsertInternal(dbContext, entities);

            await scope.SaveChangesAsync().ConfigureAwait(false);
        }

        protected void UpdateInternal(DbContext dbContext, IEnumerable<IReadModelBase> allEntities)
        {
            foreach (var entity in allEntities)
            {
                if (dbContext.Entry(entity).State != EntityState.Added)
                {
                    dbContext.Entry(entity).State = EntityState.Modified;
                }
            }
        }

        protected void InsertInternal(DbContext dbContext, IEnumerable<IReadModelBase> allEntities)
        {
            var entitygroups = allEntities
                .GroupBy(x => x.GetType())
                .ToList();

            foreach (var group in entitygroups)
            {
                var set = dbContext.Set(group.Key);

                set.AddRange(group);
            }
        }

        public async Task DeleteAsync(params IReadModelBase[] entities)
        {
            using var scope = dbContextScopeFactory.Create();
            var groups = entities
                .GroupBy(x => x.GetType())
                .ToList();

            var dbContext = scope.DbContexts.Get<TDbContext>();

            foreach (var group in groups)
            {
                foreach (var entity in group)
                {
                    if (dbContext.Entry(entity).State == EntityState.Detached)
                    {
                        dbContext.Set(group.Key).Attach(entity);
                    }
                }

                dbContext.Set(group.Key).RemoveRange(group.ToList());
            }

            await scope.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<T> GetAsync<T>(object id)
             where T : class, IReadModelBase, new()
        {
            T res = null;

            using (var scope = dbContextScopeFactory.Create())
            {
                var dbContext = scope.DbContexts.Get<TDbContext>();
                var set = dbContext.Set<T>();
                res = set.Local.FirstOrDefault(x => x.Id.Equals(id));

                res ??= set.Find(id);

                await scope.SaveChangesAsync().ConfigureAwait(false);
            }

            return res;
        }

        public async Task<object> GetAsync(Type type, object id)
        {
            object res = null;

            using (var scope = dbContextScopeFactory.Create())
            {
                var dbContext = scope.DbContexts.Get<TDbContext>();
                var set = dbContext.Set(type);
                res = set.Local.Cast<IReadModelBase>().FirstOrDefault(x => x.Id.Equals(id));

                res ??= set.Find(id);

                await scope.SaveChangesAsync().ConfigureAwait(false);
            }

            return res;
        }

        public async Task<T> GetByStreamnameAsync<T>(object streamname)
            where T : class, IStreamReadModel, new()
        {
            T res = null;

            using (var scope = dbContextScopeFactory.Create())
            {
                var local = scope.DbContexts.Get<TDbContext>()
                    .Set<T>()
                    .Local;

                res = local
                    .FirstOrDefault(x =>
                        x.Streamname != null &&
                        x.Streamname.Equals(streamname));

                res ??= scope.DbContexts.Get<TDbContext>()
                        .Set<T>()
                        .AsQueryable()
                        .Where(x =>
                            x.Streamname != null &&
                            x.Streamname == (string)streamname)
                        .FirstOrDefault();

                await scope.SaveChangesAsync().ConfigureAwait(false);
            }

            return res;
        }

        public async Task<object> GetByStreamnameAsync(Type type, object streamname)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var getByStreamnameAsyncMethodGeneric = GetType().GetRuntimeMethod(nameof(IReadRepository.GetByStreamnameAsync), new[] { typeof(object) });
            var getByStreamnameAsyncMethod = getByStreamnameAsyncMethodGeneric.MakeGenericMethod(type);

            var task = (Task)getByStreamnameAsyncMethod.Invoke(this, new object[] { streamname });
            await task.ConfigureAwait(false);

            return ((dynamic)task).Result;
        }

        public async Task<IQueryable<T>> QueryAsync<T>(Expression<Func<T, bool>> predicate)
             where T : class, IReadModelBase, new()
        {
            IQueryable<T> res = null;

            using (var scope = dbContextScopeFactory.Create())
            {
                res = scope.DbContexts.Get<TDbContext>().Set<T>().Where(predicate);
            }

            return res;
        }

        public async Task<IQueryable> QueryAsync(Type type, Expression<Func<object, bool>> predicate)
        {
            var newPredicate = ChangeInputType(predicate, type);

            var dbContext = new AmbientDbContextLocator().Get<TDbContext>(); // scope.DbContexts.Get<TDbContext>();

            var dbContextType = dbContext.GetType();

            var setMethodGeneric = dbContextType.GetTypeInfo().GetRuntimeMethod(nameof(DbContext.Set), Array.Empty<Type>());

            var setMethod = setMethodGeneric.MakeGenericMethod(type);

            var set = (IQueryable)setMethod.Invoke(dbContext, Array.Empty<object>());

            var whereMethod = typeof(Queryable).GetRuntimeMethods()
                .Where(x =>
                    x.Name == nameof(Queryable.Where))
                .First(x =>
                    x.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length == 2);

            var whereTyped = whereMethod.MakeGenericMethod(type);

            var results = (IQueryable)whereTyped.Invoke(null, new object[] { set, newPredicate });

            return results;
        }

        public IDisposable OpenScope()
        {
            var scope = dbContextScopeFactory.Create(DbContextScopeOption.ForceCreateNew);
            scope.DbContexts.Get<TDbContext>().ChangeTracker.AutoDetectChangesEnabled = false;
            return new ScopeCommitter(scope);
        }

        private static LambdaExpression ChangeInputType<T, TResult>(Expression<Func<T, TResult>> expression, Type newInputType)
        {
            if (!typeof(T).GetTypeInfo().IsAssignableFrom(newInputType.GetTypeInfo()))
            {
                throw new InvalidOperationException(string.Format("{0} is not assignable from {1}.", typeof(T), newInputType));
            }

            var beforeParameter = expression.Parameters.Single();
            var afterParameter = Expression.Parameter(newInputType, beforeParameter.Name);
            var visitor = new SubstitutionExpressionVisitor(beforeParameter, afterParameter);

            return Expression.Lambda(visitor.Visit(expression.Body), afterParameter);
        }
    }
}
