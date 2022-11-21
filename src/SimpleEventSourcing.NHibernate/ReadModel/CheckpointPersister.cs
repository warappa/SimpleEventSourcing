using NHibernate;
using NHibernate.Context;
using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.WriteModel;
using System.Threading.Tasks;

namespace SimpleEventSourcing.NHibernate.ReadModel
{
    public class CheckpointPersister<TCheckpointInfo> : CheckpointPersisterBase
        where TCheckpointInfo : class, ICheckpointInfo, new()
    {
        private readonly ISessionFactory sessionFactory;

        public CheckpointPersister(ISessionFactory sessionFactory, IPersistenceEngine engine)
            : base(engine)
        {
            this.sessionFactory = sessionFactory;
        }

        public override async Task<int> LoadLastCheckpointAsync(string projectorIdentifier)
        {
            using var session = sessionFactory.OpenSession();
            var checkpointInfo = await session.GetAsync<TCheckpointInfo>(projectorIdentifier).ConfigureAwait(false);

            if (checkpointInfo == null)
            {
                return CheckpointDefaults.NoCheckpoint;
            }

            return checkpointInfo.CheckpointNumber;
        }

        public override async Task SaveCurrentCheckpointAsync(string projectorIdentifier, int checkpoint)
        {
            var session = CurrentSessionContext.HasBind(sessionFactory) ?
                sessionFactory.GetCurrentSession() :
                sessionFactory.OpenSession();

            var checkpointInfo = await session.GetAsync<TCheckpointInfo>(projectorIdentifier).ConfigureAwait(false);

            if (checkpointInfo == null)
            {
                checkpointInfo = new TCheckpointInfo
                {
                    ProjectorIdentifier = projectorIdentifier,
                    CheckpointNumber = checkpoint
                };

                session.Save(checkpointInfo);
            }
            else
            {
                checkpointInfo.CheckpointNumber = checkpoint;

                session.Update(checkpointInfo);
            }

            // session.Flush();

            if (!CurrentSessionContext.HasBind(sessionFactory))
            {
                await session.FlushAsync().ConfigureAwait(false);
                session.Dispose();
            }
        }
    }
}
