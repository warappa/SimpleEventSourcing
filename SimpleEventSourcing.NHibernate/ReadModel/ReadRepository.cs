using NHibernate;
using NHibernate.Context;
using NHibernate.Linq;
using SimpleEventSourcing.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace SimpleEventSourcing.NHibernate.ReadModel
{
    public class ReadRepository : IReadRepository, IDbScopeAware
    {
        private readonly ISessionFactory sessionFactory;

        public ReadRepository(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        protected void UpdateInternal(ISession session, IEnumerable<IReadModelBase> allEntities)
        {
            var entitygroups = allEntities
                .GroupBy(x => x.GetType())
                .ToList();

            foreach (var group in entitygroups)
            {
                foreach (var entity in group)
                {
                    session.Update(entity);
                }
            }
        }

        protected void InsertInternal(ISession session, IEnumerable<IReadModelBase> allEntities)
        {
            var entitygroups = allEntities.GroupBy(x => x.GetType());

            foreach (var group in entitygroups)
            {
                foreach (var entity in group)
                {
                    session.Save(entity);
                }
            }
        }
        
        public Task UpdateAsync(params IReadModelBase[] entities)
        {
            using (OpenScope())
            {
                var session = sessionFactory.GetCurrentSession();

                UpdateInternal(session, entities);

                return Task.Delay(0);
            }
        }

        public Task InsertAsync(params IReadModelBase[] entities)
        {
            using (OpenScope())
            {
                var session = sessionFactory.GetCurrentSession();

                InsertInternal(session, entities);

                return Task.Delay(0);
            }
        }

        public Task DeleteAsync(params IReadModelBase[] entities)
        {
            using (OpenScope())
            {
                var session = sessionFactory.GetCurrentSession();

                foreach (var entity in entities)
                {
                    session.Delete(entity);
                }

                return Task.Delay(0);
            }
        }

        public Task<T> GetAsync<T>(object id)
             where T : class, IReadModelBase, new()
        {
            using (OpenScope())
            {
                T res = null;

                var session = sessionFactory.GetCurrentSession();

                res = session.Get<T>(id);

                return Task.FromResult(res);
            }
        }

        public Task<object> GetAsync(Type entityType, object id)
        {
            using (OpenScope())
            {
                object res = null;

                var session = sessionFactory.GetCurrentSession();

                res = session.Get(entityType, id);

                return Task.FromResult(res);
            }
        }

        public Task<T> GetByStreamnameAsync<T>(object streamname)
            where T : class, IStreamReadModel, new()
        {
            using (OpenScope())
            {
                T res = null;

                var session = sessionFactory.GetCurrentSession();
#pragma warning disable CS0253 // Möglicher unbeabsichtigter Referenzvergleich; rechte Seite muss umgewandelt werden
                res = session.Query<T>()
                    .WithOptions(x => x.SetCacheable(true))
                    .Where(x =>
                        x.Streamname != null &&
                        x.Streamname == streamname)
                    .FirstOrDefault();
#pragma warning restore CS0253 // Möglicher unbeabsichtigter Referenzvergleich; rechte Seite muss umgewandelt werden
                return Task.FromResult(res);
            }
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
            where T : class, IReadModelBase, new()
        {
            IQueryable<T> res = null;

            var session = sessionFactory.GetCurrentSession();

            res = session.Query<T>()
                .WithOptions(x => x.SetCacheable(true))
                .Where(predicate);

            return Task.FromResult(res);
        }

        public Task<IQueryable> QueryAsync(Type type, Expression<Func<object, bool>> predicate)
        {
            var session = sessionFactory.GetCurrentSession();
            var newPredicate = ChangeInputType(predicate, type);

            var queryMethod = session.GetType().GetRuntimeMethods()
                .First(x =>
                    x.Name == nameof(ISession.Query) &&
                    x.GetParameters().Length == 0)
                .MakeGenericMethod(type);

            var query = (IQueryable)queryMethod.Invoke(session, new object[0]);

            var whereMethod = typeof(Queryable).GetRuntimeMethods()
                .First(x =>
                    x.Name == nameof(Queryable.Where) &&
                    x.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length == 2)
                .MakeGenericMethod(type);

            var results = (IQueryable)whereMethod.Invoke(null, new object[] { query, newPredicate });

            return Task.FromResult(results);
        }

        public static LambdaExpression ChangeInputType<T, TResult>(Expression<Func<T, TResult>> expression, Type newInputType)
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

        public IDisposable OpenScope()
        {
            if (CurrentSessionContext.HasBind(sessionFactory))
            {
                //return new FlushDisposer(sessionFactory.GetCurrentSession());
                return new NullDisposable();
            }

            var session = sessionFactory.OpenSession();
            session.BeginTransaction();
            session.FlushMode = FlushMode.Commit;

            CurrentSessionContext.Bind(session);

            return new CurrentSessionContextDisposer(sessionFactory);
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
