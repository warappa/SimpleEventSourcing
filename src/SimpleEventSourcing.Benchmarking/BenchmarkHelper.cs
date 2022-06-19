using SimpleEventSourcing.Benchmarking.Domain;
using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.WriteModel;

namespace SimpleEventSourcing.Benchmarking
{
    public static class BenchmarkHelper
    {
        public static IRawStreamEntry[] GetRawStreamEntriesNH(this IPersistenceEngine nhPersistenceEngine, IRawStreamEntryFactory nhRawStreamEntryFactory, string streamName, IEvent[] events)
        {
            var commitId = Guid.NewGuid().ToString();

            var messages = events
                    .Select(@event => @event.ToTypedMessage(
                                   Guid.NewGuid().ToString(),
                                   new Dictionary<string, object>(),
                                   null,
                                   null,
                                   DateTime.UtcNow,
                                   0))
                    .ToList();

            var streamDTOs = messages
                .Select(x => nhRawStreamEntryFactory.CreateRawStreamEntry(nhPersistenceEngine.Serializer, streamName, commitId, 0, x))
                .ToArray();

            return streamDTOs;
        }

        public static IRawStreamEntry[] GetRawStreamEntriesEFCore(this IPersistenceEngine efCorePersistenceEngine, IRawStreamEntryFactory efCoreRawStreamEntryFactory, string streamName, IEvent[] events)
        {
            var commitId = Guid.NewGuid().ToString();

            var messages = events
                    .Select(@event => @event.ToTypedMessage(
                                   Guid.NewGuid().ToString(),
                                   new Dictionary<string, object>(),
                                   null,
                                   null,
                                   DateTime.UtcNow,
                                   0))
                    .ToList();

            var streamDTOs = messages
                .Select(x => efCoreRawStreamEntryFactory.CreateRawStreamEntry(efCorePersistenceEngine.Serializer, streamName, commitId, 0, x))
                .ToArray();

            return streamDTOs;
        }

        public static IRawStreamEntry[] GetRawStreamEntriesSQLite(this IPersistenceEngine sqlitePersistenceEngine, IRawStreamEntryFactory sqliteRawStreamEntryFactory, string streamName, IEvent[] events)
        {
            var commitId = Guid.NewGuid().ToString();

            var messages = events
                    .Select(@event => @event.ToTypedMessage(
                                   Guid.NewGuid().ToString(),
                                   new Dictionary<string, object>(),
                                   null,
                                   null,
                                   DateTime.UtcNow,
                                   0))
                    .ToList();

            var streamDTOs = messages
                .Select(x => sqliteRawStreamEntryFactory.CreateRawStreamEntry(sqlitePersistenceEngine.Serializer, streamName, commitId, 0, x))
                .ToArray();

            return streamDTOs;
        }

        public static IEvent[] GetEvents(int count = 1000)
        {
            var id = Guid.NewGuid().ToString();

            var list = new List<IEvent>();

            var limit = count;
            for (var i = 0; i < limit; i++)
            {
                if (i % 10 == 0)
                {
                    id = Guid.NewGuid().ToString();
                    list.Add(new TestAggregateCreated(id, "Test"));
                }
                else
                {
                    IEvent e;
                    if (i % 10 == 0)
                        e = new SomethingSpecialDone(id, "bla");
                    if (i % 10 == 1)
                        e = new SomethingDone(id, "bla");
                    if (i % 10 == 2)
                        e = new Renamed(id, "renamed");
                    if (i % 10 == 3)
                        e = new SomethingDone(id, "bla");
                    if (i % 10 == 4)
                        e = new SomethingSpecialDone(id, "bla");
                    if (i % 10 == 5)
                        e = new Renamed(id, "renamed");
                    if (i % 10 == 6)
                        e = new SomethingDone(id, "bla");
                    if (i % 10 == 7)
                        e = new Renamed(id, "renamed");
                    if (i % 10 == 8)
                        e = new SomethingDone(id, "bla");
                    //if (i % 10 == 9)
                    else
                        e = new Renamed(id, "renamed");
                    list.Add(e);
                }
            }

            return list.ToArray();
        }

        public static TestAggregate GenerateAggregateWithEvents()
        {
            var agg = new TestAggregate("abc" + Guid.NewGuid(), "test");

            agg.DoSomething("bla bla bla");

            agg.Rename("foo thing bar");

            agg.DoSomething("ha ha");

            agg.DoSomethingSpecial("<write poem>");

            agg.Rename("hu?");

            agg.DoSomething("hello?");

            agg.Rename("Hi!");

            return agg;
        }
    }
}