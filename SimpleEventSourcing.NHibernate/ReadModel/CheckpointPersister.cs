using NHibernate;
using System;
using System.Threading.Tasks;
using NHibernate.Context;
using SimpleEventSourcing.ReadModel;

namespace SimpleEventSourcing.NHibernate.ReadModel
{
    public class CheckpointPersister<TCheckpointInfo> : ICheckpointPersister
        where TCheckpointInfo : class, ICheckpointInfo, new()
    {
        private readonly ISessionFactory sessionFactory;

        public CheckpointPersister(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
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
            using (var session = sessionFactory.OpenSession())
            {
                var checkpointInfo = await session.GetAsync<TCheckpointInfo>(projectorIdentifier);

                if (checkpointInfo == null)
                {
                    return -1;
                }

                return checkpointInfo.CheckpointNumber;
            }
        }

        public async Task SaveCurrentCheckpointAsync(string projectorIdentifier, int checkpoint)
        {
            var session = CurrentSessionContext.HasBind(sessionFactory) ?
                sessionFactory.GetCurrentSession() :
                sessionFactory.OpenSession();

            var checkpointInfo = await session.GetAsync<TCheckpointInfo>(projectorIdentifier);

            if (checkpointInfo == null)
            {
                checkpointInfo = new TCheckpointInfo();
                checkpointInfo.StateModel = projectorIdentifier;
                checkpointInfo.CheckpointNumber = checkpoint;

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
                await session.FlushAsync();
                session.Dispose();
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
