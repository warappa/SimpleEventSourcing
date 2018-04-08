using SimpleEventSourcing.ReadModel;
using SQLite;
using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.SQLite.ReadModel
{
    public class CheckpointPersister<TCheckpointInfo> : ICheckpointPersister
        where TCheckpointInfo : class, ICheckpointInfo, new()
    {
        private readonly SQLiteConnectionWithLock connection;

        public CheckpointPersister(SQLiteConnectionWithLock connection)
        {
            this.connection = connection;
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
            TCheckpointInfo checkpointInfo = null;
            try
            {
                checkpointInfo = this.connection.Get<TCheckpointInfo>(projectorIdentifier);
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

        public void SaveCurrentCheckpoint(string projectorIdentifier, int checkpoint)
        {
            connection.RunInLock((SQLiteConnection conn) =>
            {
                var cmd = conn.CreateCommand("update checkpointinfo set checkpointnumber=@p0 where checkpointinfo.statemodel=@p1");
                cmd.Bind("@p0", checkpoint);
                cmd.Bind("@p1", projectorIdentifier);

                var res = cmd.ExecuteNonQuery();

                if (res == 0)
                {
                    cmd.CommandText = "insert into checkpointinfo (statemodel, checkpointnumber) values (@p0, @p1)";
                    cmd.Bind("@p0", projectorIdentifier);
                    cmd.Bind("@p1", checkpoint);
                    cmd.ExecuteNonQuery();
                }
            });
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
