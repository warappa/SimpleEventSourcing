using SimpleEventSourcing.State;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleEventSourcing.ReadModel
{
    public interface ICheckpointPersister
    {
        Task<int> LoadLastCheckpointAsync(string projectorIdentifier);
        Task ResetCheckpointAsync(string projectorIdentifier);
        Task SaveCurrentCheckpointAsync(string projectorIdentifier, int checkpoint);
        string GetProjectorIdentifier<T>();
        string GetProjectorIdentifier(Type projectorType);
        Task WaitForCheckpointNumberAsync(Type readModelProjectorType, int checkpointNumber, CancellationToken token = default);
        Task WaitForCheckpointNumberAsync(Type readModelProjectorType, int checkpointNumber, TimeSpan timeout, CancellationToken token = default);
        Task WaitForCheckpointNumberAsync<TReadModelState>(int checkpointNumber, CancellationToken token = default)
            where TReadModelState : IAsyncProjector;
        Task WaitForCheckpointNumberAsync<TReadModelState>(int checkpointNumber, TimeSpan timeout, CancellationToken token = default)
            where TReadModelState : IAsyncProjector;
    }
}
