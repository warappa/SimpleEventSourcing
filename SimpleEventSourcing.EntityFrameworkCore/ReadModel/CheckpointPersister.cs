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

        public int LoadLastCheckpoint(string projectorIdentifier)
        {
            using (var scope = dbContextScopeFactory.Create())
            {
                TCheckpointInfo checkpointInfo = null;
                try
                {
                    checkpointInfo = scope.DbContexts.Get<TDbContext>().Set<TCheckpointInfo>()
                        .Find(projectorIdentifier);
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

        public void SaveCurrentCheckpoint(string projectorIdentifier, int checkpoint)
        {
            using (var scope = dbContextScopeFactory.Create())
            {
                var set = scope.DbContexts.Get<TDbContext>().Set<TCheckpointInfo>();
                var checkpointInfo = set.Find(projectorIdentifier);

                if (checkpointInfo == null)
                {
                    checkpointInfo = new TCheckpointInfo();
                    checkpointInfo.StateModel = projectorIdentifier;
                    checkpointInfo.CheckpointNumber = checkpoint;
                    set.Add(checkpointInfo);
                }
                else
                {
                    checkpointInfo.CheckpointNumber = checkpoint;
                }

                scope.SaveChanges();
            }
        }

        public async Task WaitForCheckpointNumberAsync<TReadModelState>(int checkpointNumber)
        {
            var timeout = DateTime.Now.AddSeconds(60);

            var lastLoadedCheckpoint = LoadLastCheckpoint(GetProjectorIdentifier<TReadModelState>());

            while (DateTime.Now < timeout &&
                lastLoadedCheckpoint < checkpointNumber)
            {
                await Task.Delay(100).ConfigureAwait(false);
                lastLoadedCheckpoint = LoadLastCheckpoint(GetProjectorIdentifier<TReadModelState>());
            }
        }
    }
}
