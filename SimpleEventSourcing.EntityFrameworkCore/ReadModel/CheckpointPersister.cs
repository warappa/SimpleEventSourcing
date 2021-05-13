using EntityFrameworkCore.DbContextScope;
using Microsoft.EntityFrameworkCore;
using SimpleEventSourcing.ReadModel;
using System.Threading.Tasks;

namespace SimpleEventSourcing.EntityFrameworkCore.ReadModel
{
    public class CheckpointPersister<TDbContext, TCheckpointInfo> : CheckpointPersisterBase
        where TDbContext : DbContext
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

        public override async Task SaveCurrentCheckpointAsync(string projectorIdentifier, int checkpoint)
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
    }
}
