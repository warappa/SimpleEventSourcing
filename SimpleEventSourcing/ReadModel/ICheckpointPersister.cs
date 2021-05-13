using SimpleEventSourcing.State;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleEventSourcing.ReadModel
{
    public interface ICheckpointPersister
    {
        Task<int> LoadLastCheckpointAsync(string projectorIdentifier);
        Task SaveCurrentCheckpointAsync(string projectorIdentifier, int checkpoint);
        string GetProjectorIdentifier<T>();
        string GetProjectorIdentifier(Type projectorType);
        Task WaitForCheckpointNumberAsync(Type readModelStateType, int checkpointNumber, CancellationToken token = default);
        Task WaitForCheckpointNumberAsync<TReadModelState>(int checkpointNumber, CancellationToken token = default)
            where TReadModelState : IState;
    }
}
