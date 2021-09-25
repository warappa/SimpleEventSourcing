using SimpleEventSourcing.NHibernate.WriteModel.Tests;
using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.NHibernate.Tests
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var t = new PersistenceEngineBulkCheckpointNumberTests();
            await t.TestSetupAsync();
            await t.SetupTest();

            await t.Entities_are_in_the_same_order_as_they_were_inserted_checked_by_checkpointnumber();

            Console.ReadKey();
        }
    }
}