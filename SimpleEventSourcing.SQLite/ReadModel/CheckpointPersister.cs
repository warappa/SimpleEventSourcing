﻿using SimpleEventSourcing.ReadModel;
using SQLite;
using System.Threading.Tasks;

namespace SimpleEventSourcing.SQLite.ReadModel
{
    public class CheckpointPersister<TCheckpointInfo> : CheckpointPersisterBase
        where TCheckpointInfo : class, ICheckpointInfo, new()
    {
        private readonly SQLiteConnectionWithLock connection;

        public CheckpointPersister(SQLiteConnectionWithLock connection)
        {
            this.connection = connection;
        }

        public override async Task<int> LoadLastCheckpointAsync(string projectorIdentifier)
        {
            TCheckpointInfo checkpointInfo = null;
            try
            {
                checkpointInfo = connection.Get<TCheckpointInfo>(projectorIdentifier);
            }
            catch
            {
                try
                {
                    if (!connection.TableExists("CheckpointInfos"))
                    {
                        connection.CreateTable(typeof(CheckpointInfo));
                        checkpointInfo = connection.Get<TCheckpointInfo>(projectorIdentifier);
                    }
                }
                catch
                {

                }
                // TODO: error handling
            }

            if (checkpointInfo == null)
            {
                return -1;
            }

            return checkpointInfo.CheckpointNumber;
        }

        public override async Task SaveCurrentCheckpointAsync(string projectorIdentifier, int checkpoint)
        {
            void Run()
            {
                connection.RunInLock((SQLiteConnection conn) =>
                {
                    var cmd = conn.CreateCommand("UPDATE CheckpointInfos SET checkpointnumber=@p0 WHERE CheckpointInfos.statemodel=@p1");
                    cmd.Bind("@p0", checkpoint);
                    cmd.Bind("@p1", projectorIdentifier);

                    var res = cmd.ExecuteNonQuery();

                    if (res == 0)
                    {
                        cmd.CommandText = "INSERT INTO CheckpointInfos (statemodel, checkpointnumber) VALUES (@p0, @p1)";
                        cmd.Bind("@p0", projectorIdentifier);
                        cmd.Bind("@p1", checkpoint);
                        cmd.ExecuteNonQuery();
                    }
                });
            }

            try
            {
                Run();
            }
            catch
            {
                if (!connection.TableExists("CheckpointInfos"))
                {
                    connection.CreateTable(typeof(CheckpointInfo));
                    Run();
                }
            }
        }
    }
}
