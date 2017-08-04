using System;
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

        public int LoadLastCheckpoint(string projectorIdentifier)
        {
            return -1;
        }

        public void SaveCurrentCheckpoint(string projectorIdentifier, int checkpoint)
        {
            // does nothing
        }

        public Task WaitForCheckpointNumberAsync<TReadModelState>(int checkpointNumber)
        {
            return Task.Delay(0);
        }
    }
}
