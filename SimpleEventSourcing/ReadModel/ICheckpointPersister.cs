using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.ReadModel
{
    public interface ICheckpointPersister
    {
        int LoadLastCheckpoint(string projectorIdentifier);
        void SaveCurrentCheckpoint(string projectorIdentifier, int checkpoint);
        string GetProjectorIdentifier<T>();
        string GetProjectorIdentifier(Type projectorType);
        Task WaitForCheckpointNumberAsync<TReadModelState>(int checkpointNumber);
    }
}
