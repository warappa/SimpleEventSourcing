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
            await SaveCurrentCheckpointAsync(projectorIdentifier, CheckpointDefaults.NoCheckpoint);
        }

        public abstract Task<int> LoadLastCheckpointAsync(string projectorIdentifier);

        public abstract Task SaveCurrentCheckpointAsync(string projectorIdentifier, int checkpoint);

        public async Task WaitForCheckpointNumberAsync<TReadModelState>(int checkpointNumber, CancellationToken token = default)
            where TReadModelState : IAsyncState
        {
            await WaitForCheckpointNumberAsync(typeof(TReadModelState), checkpointNumber, token);
        }

        public async Task WaitForCheckpointNumberAsync(Type readModelStateType, int checkpointNumber, CancellationToken token = default)
        {
            var projectorIdentifier = GetProjectorIdentifier(readModelStateType);

            var retries = 0;
            var pow = 1;
            var delayInMs = 16;
            var maxDelayMs = 1000;

            while (true)
            {
                token.ThrowIfCancellationRequested();

                var lastLoadedCheckpoint = await LoadLastCheckpointAsync(projectorIdentifier).ConfigureAwait(false);

                if (lastLoadedCheckpoint >= checkpointNumber)
                {
                    return;
                }

                retries++;

                if (retries == 20)
                {
                    throw new OperationCanceledException($"Projector '{readModelStateType.FullName}' did not reach checkpoint '{checkpointNumber}'.");
                }

                if (retries < 31)
                {
                    pow <<= 1;
                }

                var delay = Math.Min(delayInMs * (pow - 1) / 2, maxDelayMs);
                await Task.Delay(delay, token).ConfigureAwait(false);
            }
        }
    }
}
