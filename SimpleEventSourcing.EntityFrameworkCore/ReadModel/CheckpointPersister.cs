using EntityFrameworkCore.DbContextScope;
using Microsoft.EntityFrameworkCore;
using SimpleEventSourcing.ReadModel;
using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.EntityFrameworkCore.ReadModel
{
    public class CheckpointPersister<TDbContext, TCheckpointInfo> : ICheckpointPersister
        where TDbContext : DbContext
        where TCheckpointInfo : class, ICheckpointInfo, new()
    {
        private readonly IDbContextScopeFactory dbContextScopeFactory;

        public CheckpointPersister(IDbContextScopeFactory dbContextScopeFactory)
        {
            this.dbContextScopeFactory = dbContextScopeFactory;
        }

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
            using (var scope = dbContextScopeFactory.Create())
            {
                TCheckpointInfo checkpointInfo = null;
                try
                {
                    checkpointInfo = await scope.DbContexts.Get<TDbContext>()
                        .Set<TCheckpointInfo>()
                        .FindAsync(projectorIdentifier);
                }
                catch
                {
                    // TODO: error handling
                }

                if (checkpointInfo == null)
                {
                    return -1;
                }

                return checkpointInfo.CheckpointNumber;
            }
        }

        public async Task SaveCurrentCheckpointAsync(string projectorIdentifier, int checkpoint)
        {
            using (var scope = dbContextScopeFactory.Create())
            {
                var set = scope.DbContexts.Get<TDbContext>()
                    .Set<TCheckpointInfo>();
                var checkpointInfo = await set.FindAsync(projectorIdentifier);

                if (checkpointInfo == null)
                {
                    checkpointInfo = new TCheckpointInfo
                    {
                        StateModel = projectorIdentifier,
                        CheckpointNumber = checkpoint
                    };
                    set.Add(checkpointInfo);
                }
                else
                {
                    checkpointInfo.CheckpointNumber = checkpoint;
                }

                await scope.SaveChangesAsync();
            }
        }

        public async Task WaitForCheckpointNumberAsync<TReadModelState>(int checkpointNumber)
        {
            var timeout = DateTime.Now.AddSeconds(60);

            var projectorIdentifier = GetProjectorIdentifier<TReadModelState>();
            var lastLoadedCheckpoint = await LoadLastCheckpointAsync(projectorIdentifier);

            while (DateTime.Now < timeout &&
                lastLoadedCheckpoint < checkpointNumber)
            {
                await Task.Delay(100).ConfigureAwait(false);
                lastLoadedCheckpoint = await LoadLastCheckpointAsync(projectorIdentifier);
            }
        }
    }
}
