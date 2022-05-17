using SimpleEventSourcing.State;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleEventSourcing.ReadModel
{
    public abstract class CheckpointPersisterBase : ICheckpointPersister
    {
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

        public async Task WaitForCheckpointNumberAsync<TReadModelState>(int checkpointNumber, CancellationToken token = default)
            where TReadModelState : IAsyncState
        {
            await WaitForCheckpointNumberAsync(typeof(TReadModelState), checkpointNumber, token).ConfigureAwait(false);
        }

        public async Task WaitForCheckpointNumberAsync<TReadModelState>(int checkpointNumber, TimeSpan timeout, CancellationToken token = default)
            where TReadModelState : IAsyncState
        {
            await WaitForCheckpointNumberAsync(typeof(TReadModelState), checkpointNumber, timeout, token).ConfigureAwait(false);
        }

        public async Task WaitForCheckpointNumberAsync(Type readModelStateType, int checkpointNumber, CancellationToken token = default)
        {
            await WaitForCheckpointNumberInternalAsync(readModelStateType, checkpointNumber, null, token).ConfigureAwait(false);
        }
        public async Task WaitForCheckpointNumberAsync(Type readModelStateType, int checkpointNumber, TimeSpan timeout, CancellationToken token = default)
        {
            await WaitForCheckpointNumberInternalAsync(readModelStateType, checkpointNumber, timeout, token).ConfigureAwait(false);
        }

        internal async Task WaitForCheckpointNumberInternalAsync(Type readModelStateType, int checkpointNumber, TimeSpan? timeout = null, CancellationToken token = default)
        {
            var projectorIdentifier = GetProjectorIdentifier(readModelStateType);

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
                    throw new OperationCanceledException($"Projector '{readModelStateType.FullName}' did not reach checkpoint '{checkpointNumber}'.");
                }

                //if (retries < 31)
                //{
                //    pow <<= 1;
                //}

                var delay = Math.Min(delayInMs * (pow - 1) / 2, maxDelayMs);
                await Task.Delay(delay, token).ConfigureAwait(false);
            }
        }
    }
}
