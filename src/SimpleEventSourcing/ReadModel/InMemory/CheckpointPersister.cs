using SimpleEventSourcing.WriteModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleEventSourcing.ReadModel.InMemory
{
    public class CheckpointPersister : CheckpointPersisterBase
    {
        private readonly Dictionary<string, int> checkpoints = new Dictionary<string, int>();

        public CheckpointPersister(IPersistenceEngine engine)
            : base(engine)
        {
        }

        public override async Task<int> LoadLastCheckpointAsync(string projectorIdentifier)
        {
            if (checkpoints.TryGetValue(projectorIdentifier, out var checkpoint))
            {
                return checkpoint;
            }

            return CheckpointDefaults.NoCheckpoint;
        }

        public override async Task SaveCurrentCheckpointAsync(string projectorIdentifier, int checkpoint)
        {
            checkpoints[projectorIdentifier] = checkpoint;
        }
    }
}
