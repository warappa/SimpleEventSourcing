using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SimpleEventSourcing.ReadModel
{
    public interface IReadRepository
    {
        Task DeleteAsync(params IReadModelBase[] entities);

        Task<T> GetAsync<T>(object id) where T : class, IReadModelBase, new();
        Task<object> GetAsync(Type entityType, object id);

        Task<T> GetByStreamnameAsync<T>(object streamname) where T : class, IStreamReadModel, new();
        Task<object> GetByStreamnameAsync(Type entityType, object streamname);

        Task<IQueryable<T>> QueryAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IReadModelBase, new();
        Task<IQueryable> QueryAsync(Type type, Expression<Func<object, bool>> predicate);

        Task UpdateAsync(params IReadModelBase[] entities);

        Task InsertAsync(params IReadModelBase[] entities);
    }
}
