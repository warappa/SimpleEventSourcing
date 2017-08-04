using SimpleEventSourcing.ReadModel;
using SQLite.Net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace SimpleEventSourcing.SQLite.ReadModel
{
    public class ReadRepository : IReadRepository, IDbScopeAware
    {
        private static readonly MethodInfo getGenericMethodInfo;
        private static readonly MethodInfo tableGenericMethodInfo;

        private readonly Func<SQLiteConnectionWithLock> connectionFactory;

        static ReadRepository()
        {
            var connectionType = typeof(SQLiteConnectionWithLock);

            getGenericMethodInfo = connectionType.GetRuntimeMethod(nameof(SQLiteConnectionWithLock.Get), new[] { typeof(object) });
            tableGenericMethodInfo = connectionType.GetRuntimeMethod(nameof(SQLiteConnectionWithLock.Table), new Type[0]);
        }

        public ReadRepository(Func<SQLiteConnectionWithLock> connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        protected void InsertInternal<T>(SQLiteConnection conn, IEnumerable<T> entities) where T : class, IReadModelBase
        {
            foreach (var entity in entities)
            {
                conn.Insert(entity);
            }
        }

        protected void UpdateInternal<T>(SQLiteConnection conn, IEnumerable<T> entities) where T : class, IReadModelBase
        {
            foreach (var entity in entities)
            {
                conn.Update(entity);
            }
        }

        public Task UpdateAsync(params IReadModelBase[] entities)
        {
            var conn = connectionFactory();
            conn.RunInLock(() => UpdateInternal(conn, entities));

            return Task.Delay(0);
        }

        public Task InsertAsync(params IReadModelBase[] entities)
        {
            var conn = connectionFactory();
            conn.RunInLock(() => InsertInternal(conn, entities));

            return Task.Delay(0);
        }

        public Task DeleteAsync(params IReadModelBase[] entities)
        {
            var conn = connectionFactory();
            conn.RunInLock(() =>
            {
                foreach (var e in entities)
                {
                    conn.Delete(e);
                }
            });

            return Task.Delay(0);
        }

        public Task<T> GetAsync<T>(object id)
            where T : class, IReadModelBase, new()
        {
            var conn = connectionFactory();
            T res = null;
            conn.RunInLock(() => res = conn.Get<T>(id));

            return Task.FromResult(res);
        }

        public Task<object> GetAsync(Type entityType, object id)
        {
            var conn = connectionFactory();
            object res = null;
            conn.RunInLock(() =>
            {
                var getMethodInfo = getGenericMethodInfo.MakeGenericMethod(entityType);
                res = getMethodInfo.Invoke(conn, new[] { id });
            });

            return Task.FromResult(res);
        }

        public Task<T> GetByStreamnameAsync<T>(object streamname)
            where T : class, IStreamReadModel, new()
        {
            var conn = connectionFactory();
            T res = null;
            conn.RunInLock(() =>
            {
                res = conn.Table<T>()
                    .Where(x =>
                        x.Streamname != null &&
                        x.Streamname.Equals(streamname))
                    .FirstOrDefault();
            });

            return Task.FromResult(res);
        }

        public async Task<object> GetByStreamnameAsync(Type entityType, object streamname)
        {
            var getByStreamnameAsyncMethodGeneric = GetType().GetRuntimeMethod(nameof(IReadRepository.GetByStreamnameAsync), new[] { typeof(object) });
            var getByStreamnameAsyncMethod = getByStreamnameAsyncMethodGeneric.MakeGenericMethod(entityType);

            var task = (Task)(getByStreamnameAsyncMethod.Invoke(this, new object[] { streamname }));
            await task;
            
            return ((dynamic)task).Result;
        }

        public Task<IQueryable<T>> QueryAsync<T>(Expression<Func<T, bool>> predicate)
            where T : class, IReadModelBase
        {
            var conn = connectionFactory();

            var res = conn.Table<T>().Where(predicate).AsQueryable();

            return Task.FromResult(res);
        }

        public Task<IQueryable> QueryAsync(Type type, Expression<Func<object, bool>> predicate)
        {
            var conn = connectionFactory();
            IQueryable res = null;

            var tableMethodInfo = tableGenericMethodInfo.MakeGenericMethod(type);
            var tableQuery = tableMethodInfo.Invoke(conn, new object[0]);

            var newPredicate = ChangeInputType(predicate, type);

            var whereMethodInfo2 = tableQuery.GetType().GetTypeInfo().GetDeclaredMethods(nameof(TableQuery<object>.Where))
                .First(x => x.GetGenericArguments().Length == 0);

            var results = (IEnumerable)whereMethodInfo2.Invoke(tableQuery, new object[] { newPredicate });

            res = results.AsQueryable();

            return Task.FromResult(res);
        }

        public IDisposable OpenScope()
        {
            var conn = connectionFactory();

            var @lock = conn.Lock();
            conn.BeginTransaction();

            return new TransactionDisposer(conn, @lock);
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

        private class TransactionDisposer : IDisposable
        {
            private readonly SQLiteConnectionWithLock connection;
            private readonly IDisposable @lock;

            public TransactionDisposer(SQLiteConnectionWithLock connection, IDisposable @lock)
            {
                this.connection = connection;
                this.@lock = @lock;
            }

            public void Dispose()
            {
                Dispose(true);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    connection.Commit();
                    @lock.Dispose();
                }
            }
        }

        private class SubstitutionExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression before;
            private readonly Expression after;

            public SubstitutionExpressionVisitor(Expression before, Expression after)
            {
                this.before = before;
                this.after = after;
            }

            public override Expression Visit(Expression node)
            {
                return node == before ? after : base.Visit(node);
            }
        }
    }
}
