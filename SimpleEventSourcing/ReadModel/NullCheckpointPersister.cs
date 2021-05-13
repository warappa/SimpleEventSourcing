using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleEventSourcing.ReadModel
{
    public class NullCheckpointPersister : ICheckpointPersister
    {
        public string GetProjectorIdentifier<T>()
        {
            return GetProjectorIdentifier(typeof(T));
        }

        public string GetProjectorIdentifier(Type projectorType)
        {
            return projectorType.Name;
        }

        public async Task<int> LoadLastCheckpointAsync(string projectorIdentifier)
        {
            return -1;
        }

        public Task SaveCurrentCheckpointAsync(string projectorIdentifier, int checkpoint)
        {
            // does nothing
            return Task.CompletedTask;
        }

        public Task WaitForCheckpointNumberAsync<TReadModelState>(int checkpointNumber, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        public Task WaitForCheckpointNumberAsync(Type readModelStateType, int checkpointNumber, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }
    }
}
