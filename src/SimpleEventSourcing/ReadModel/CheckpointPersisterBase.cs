using SimpleEventSourcing.State;
using SimpleEventSourcing.WriteModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleEventSourcing.ReadModel
{
    public abstract class CheckpointPersisterBase : ICheckpointPersister
    {
        private readonly IPersistenceEngine engine;

        protected CheckpointPersisterBase(IPersistenceEngine engine)
        {
            this.engine = engine ?? throw new ArgumentNullException(nameof(engine));
        }

        public virtual string GetProjectorIdentifier<T>()
        {
            return GetProjectorIdentifier(typeof(T));
        }

        public virtual string GetProjectorIdentifier(Type projectorType)
        {
            return projectorType.Name;
        }

        public async Task ResetCheckpointAsync(string projectorIdentifier)
        {
            await SaveCurrentCheckpointAsync(projectorIdentifier, CheckpointDefaults.NoCheckpoint).ConfigureAwait(false);
        }

        public abstract Task<int> LoadLastCheckpointAsync(string projectorIdentifier);

        public abstract Task SaveCurrentCheckpointAsync(string projectorIdentifier, int checkpoint);

        public async Task WaitForCheckpointNumberAsync<TReadModelProjector>(int checkpointNumber, CancellationToken token = default)
            where TReadModelProjector : IAsyncProjector
        {
            await WaitForCheckpointNumberAsync(typeof(TReadModelProjector), checkpointNumber, token).ConfigureAwait(false);
        }

        public async Task WaitForCheckpointNumberAsync<TReadModelProjector>(int checkpointNumber, TimeSpan timeout, CancellationToken token = default)
            where TReadModelProjector : IAsyncProjector
        {
            await WaitForCheckpointNumberAsync(typeof(TReadModelProjector), checkpointNumber, timeout, token).ConfigureAwait(false);
        }

        public async Task WaitForCheckpointNumberAsync(Type readModelProjectorType, int checkpointNumber, CancellationToken token = default)
        {
            await WaitForCheckpointNumberInternalAsync(readModelProjectorType, checkpointNumber, null, token).ConfigureAwait(false);
        }
        public async Task WaitForCheckpointNumberAsync(Type readModelProjectorType, int checkpointNumber, TimeSpan timeout, CancellationToken token = default)
        {
            await WaitForCheckpointNumberInternalAsync(readModelProjectorType, checkpointNumber, timeout, token).ConfigureAwait(false);
        }

        internal async Task WaitForCheckpointNumberInternalAsync(Type readModelProjectorType, int checkpointNumber, TimeSpan? timeout = null, CancellationToken token = default)
        {
            var projectorIdentifier = GetProjectorIdentifier(readModelProjectorType);

            //var retries = 0;
            var pow = 1;
            var delayInMs = 16;
            var maxDelayMs = 1000;

            if (timeout is null)
            {
                timeout = TimeSpan.FromSeconds(10);
            }

            var endTime = DateTime.UtcNow + timeout;

            while (true)
            {
                token.ThrowIfCancellationRequested();

                var lastLoadedCheckpoint = await LoadLastCheckpointAsync(projectorIdentifier).ConfigureAwait(false);

                if (lastLoadedCheckpoint >= checkpointNumber)
                {
                    return;
                }

                //retries++;

                //if (retries == 20)
                if (DateTime.UtcNow > endTime)
                {
                    throw new OperationCanceledException($"Projector '{readModelProjectorType.FullName}' did not reach checkpoint '{checkpointNumber}'.");
                }

                //if (retries < 31)
                //{
                //    pow <<= 1;
                //}

                var delay = Math.Min(delayInMs * (pow - 1) / 2, maxDelayMs);
                await Task.Delay(delay, token).ConfigureAwait(false);
            }
        }

        public async Task WaitForCurrentCheckpointNumberAsync(Type readModelProjectorType, CancellationToken token = default)
        {
            var cp = await engine.GetCurrentEventStoreCheckpointNumberAsync();
            await WaitForCheckpointNumberAsync(readModelProjectorType, cp, token);
        }

        public async Task WaitForCurrentCheckpointNumberAsync(Type readModelProjectorType, TimeSpan timeout, CancellationToken token = default)
        {
            var cp = await engine.GetCurrentEventStoreCheckpointNumberAsync();
            await WaitForCheckpointNumberAsync(readModelProjectorType, cp, timeout, token);
        }

        public async Task WaitForCurrentCheckpointNumberAsync<TReadModelProjector>(CancellationToken token = default) where TReadModelProjector : IAsyncProjector
        {
            await WaitForCurrentCheckpointNumberAsync(typeof(TReadModelProjector), token);
        }

        public async Task WaitForCurrentCheckpointNumberAsync<TReadModelProjector>(TimeSpan timeout, CancellationToken token = default) where TReadModelProjector : IAsyncProjector
        {
            await WaitForCurrentCheckpointNumberAsync(typeof(TReadModelProjector), timeout, token);
        }
    }
}
