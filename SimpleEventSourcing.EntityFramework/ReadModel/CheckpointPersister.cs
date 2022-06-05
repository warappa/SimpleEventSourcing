using EntityFramework.DbContextScope.Interfaces;
using SimpleEventSourcing.ReadModel;
using System.Data.Entity;
using System.Threading.Tasks;

namespace SimpleEventSourcing.EntityFramework.ReadModel
{
    public class CheckpointPersister<TDbContext, TCheckpointInfo> : CheckpointPersisterBase
        where TDbContext : DbContext, IDbContext
        where TCheckpointInfo : class, ICheckpointInfo, new()
    {
        private readonly IDbContextScopeFactory dbContextScopeFactory;

        public CheckpointPersister(IDbContextScopeFactory dbContextScopeFactory)
        {
            this.dbContextScopeFactory = dbContextScopeFactory;
        }

        public override async Task<int> LoadLastCheckpointAsync(string projectorIdentifier)
        {
            using (var scope = dbContextScopeFactory.Create())
            {
                TCheckpointInfo checkpointInfo = null;
                try
                {
                    checkpointInfo = await scope.DbContexts.Get<TDbContext>()
                        .Set<TCheckpointInfo>()
                        .FindAsync(projectorIdentifier)
                        .ConfigureAwait(false);
                }
                catch
                {
                    // TODO: error handling
                }

                if (checkpointInfo == null)
                {
                    return CheckpointDefaults.NoCheckpoint;
                }

                return checkpointInfo.CheckpointNumber;
            }
        }

        public override async Task SaveCurrentCheckpointAsync(string projectorIdentifier, int checkpoint)
        {
            using (var scope = dbContextScopeFactory.Create())
            {
                var set = scope.DbContexts.Get<TDbContext>()
                    .Set<TCheckpointInfo>();
                var checkpointInfo = await set.FindAsync(projectorIdentifier).ConfigureAwait(false);

                if (checkpointInfo == null)
                {
                    checkpointInfo = new TCheckpointInfo
                    {
                        ProjectorIdentifier = projectorIdentifier,
                        CheckpointNumber = checkpoint
                    };
                    set.Add(checkpointInfo);
                }
                else
                {
                    checkpointInfo.CheckpointNumber = checkpoint;
                }

                await scope.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}
