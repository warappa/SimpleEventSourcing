﻿using SimpleEventSourcing.State;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SimpleEventSourcing.ReadModel
{
    public class ReadRepositoryProjector<TProjector> : AsyncEventSourcedProjector<TProjector>,
        IDbScopeAware, IReadRepositoryProjector<TProjector>
        where TProjector : ReadRepositoryProjector<TProjector>, new()
    {
        protected IReadRepository readRepository;

        public ReadRepositoryProjector(IReadRepository readRepository)
        {
            this.readRepository = readRepository;
        }

        public ReadRepositoryProjector()
        {
            throw new NotSupportedException();
        }

        public Task<TReadModel> GetByStreamName<TReadModel>(string streamName)
            where TReadModel : class, IStreamReadModel, new()
        {
            return readRepository.GetByStreamnameAsync<TReadModel>(streamName);
        }

        public async Task InsertAsync(IReadModelBase readModel)
        {
            await readRepository.InsertAsync(readModel).ConfigureAwait(false);
        }

        public async Task UpdateAsync(IReadModelBase readModel)
        {
            await readRepository.UpdateAsync(readModel).ConfigureAwait(false);
        }

        public async Task UpdateByIdAsync<TReadModel, TKey>(TKey id, Action<TReadModel> action)
            where TReadModel : class, IReadModel<TKey>, new()
        {
            var readModel = await readRepository.GetAsync<TReadModel>(id).ConfigureAwait(false);

            action(readModel);

            await readRepository.UpdateAsync(readModel).ConfigureAwait(false);
        }

        public async Task UpdateByIdAsync<TReadModel, TKey>(TKey id, Func<TReadModel, Task> action)
            where TReadModel : class, IReadModel<TKey>, new()
        {
            var readModel = await readRepository.GetAsync<TReadModel>(id).ConfigureAwait(false);

            await action(readModel).ConfigureAwait(false);

            await readRepository.UpdateAsync(readModel).ConfigureAwait(false);
        }

        public async Task UpdateByStreamnameAsync<TReadModel>(string streamName, Action<TReadModel> action)
            where TReadModel : class, IStreamReadModel, new()
        {
            var readModel = await readRepository.GetByStreamnameAsync<TReadModel>(streamName).ConfigureAwait(false);

            action(readModel);

            await readRepository.UpdateAsync(readModel).ConfigureAwait(false);
        }

        public async Task UpdateByStreamnameAsync<TReadModel>(string streamName, Func<TReadModel, Task> action)
            where TReadModel : class, IStreamReadModel, new()
        {
            var readModel = await readRepository.GetByStreamnameAsync<TReadModel>(streamName).ConfigureAwait(false);

            await action(readModel).ConfigureAwait(false);

            await readRepository.UpdateAsync(readModel).ConfigureAwait(false);
        }

        public async Task QueryAndUpdateAsync<TReadModel>(Expression<Func<TReadModel, bool>> predicate, Action<TReadModel> action)
            where TReadModel : class, IReadModelBase, new()
        {
            var readModels = (await readRepository.QueryAsync(predicate).ConfigureAwait(false))
                .ToList();

            foreach (var readModel in readModels)
            {
                action(readModel);

                await readRepository.UpdateAsync(readModel).ConfigureAwait(false);
            }
        }

        public async Task QueryAndUpdateAsync<TReadModel>(Expression<Func<TReadModel, bool>> predicate, Func<TReadModel, Task> action)
            where TReadModel : class, IReadModelBase, new()
        {
            var readModels = (await readRepository.QueryAsync(predicate).ConfigureAwait(false))
                .ToList();

            foreach (var readModel in readModels)
            {
                await action(readModel).ConfigureAwait(false);

                await readRepository.UpdateAsync(readModel).ConfigureAwait(false);
            }
        }

        public IDisposable OpenScope()
        {
            return (readRepository as IDbScopeAware)?.OpenScope() ?? new NullDisposable();
        }
    }
}
