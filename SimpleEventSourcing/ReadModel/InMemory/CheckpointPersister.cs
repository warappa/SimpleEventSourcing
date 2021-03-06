﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleEventSourcing.ReadModel.InMemory
{
    public class CheckpointPersister : CheckpointPersisterBase
    {
        private Dictionary<string, int> checkpoints = new Dictionary<string, int>();

        public override async Task<int> LoadLastCheckpointAsync(string projectorIdentifier)
        {
            if (checkpoints.TryGetValue(projectorIdentifier, out var checkpoint))
            {
                return checkpoint;
            }

            return -1;
        }

        public override async Task SaveCurrentCheckpointAsync(string projectorIdentifier, int checkpoint)
        {
            checkpoints[projectorIdentifier] = checkpoint;
        }
    }
}
