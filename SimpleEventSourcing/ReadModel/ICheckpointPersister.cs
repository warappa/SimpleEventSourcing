﻿using SimpleEventSourcing.State;
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
        Task WaitForCheckpointNumberAsync<TReadModelProjector>(int checkpointNumber, CancellationToken token = default)
            where TReadModelProjector : IAsyncProjector;
        Task WaitForCheckpointNumberAsync<TReadModelProjector>(int checkpointNumber, TimeSpan timeout, CancellationToken token = default)
            where TReadModelProjector : IAsyncProjector;
    }
}
