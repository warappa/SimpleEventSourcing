using FluentAssertions;
using NUnit.Framework;
using SimpleEventSourcing.State;
using SimpleEventSourcing.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleEventSourcing.WriteModel.Tests
{
    [TestFixture]
    public abstract class PersistenceEngineBasicTestsBase : PersistenceEngineTestsBase
    {
        protected PersistenceEngineBasicTestsBase(TestsBaseConfig config, bool initialize = true)
            : base(config, initialize)
        {

        }

        [Test]
        public async Task Can_initializeAsync()
        {
            await InitializeAsync().ConfigureAwait(false);
        }

        [Test]
        public async Task Can_save_streamEntriesAsync()
        {
            await InitializeAsync().ConfigureAwait(false);

            await SaveStreamEntryAsync().ConfigureAwait(false);
        }

        [Test]
        public async Task Can_load_snapshot_respecting_maxRevision()
        {
            await InitializeAsync().ConfigureAwait(false);

            var streamName = "snapshottest";
            var entries = GetStreamEntries(streamName, 100);

            var snapShot10 = new SnapshotTestState(streamName);
            snapShot10 = entries.Take(10).ApplyToState(snapShot10, persistenceEngine.Serializer);

            var snapShot100 = new SnapshotTestState(streamName);
            snapShot100 = entries.Take(100).ApplyToState(snapShot100, persistenceEngine.Serializer);

            await persistenceEngine.SaveSnapshotAsync(snapShot10, 10);
            await persistenceEngine.SaveSnapshotAsync(snapShot100, 100);

            var loaded1Raw = await persistenceEngine.LoadLatestSnapshotAsync(streamName, "SnapshotTestState|0", 1);
            var loaded10Raw = await persistenceEngine.LoadLatestSnapshotAsync(streamName, "SnapshotTestState|0", 10);
            var loaded100Raw = await persistenceEngine.LoadLatestSnapshotAsync(streamName, "SnapshotTestState|0", 100);

            var loaded10 = persistenceEngine.Serializer.Deserialize<SnapshotTestState>(loaded10Raw.StateSerialized);
            var loaded100 = persistenceEngine.Serializer.Deserialize<SnapshotTestState>(loaded100Raw.StateSerialized);

            loaded1Raw.Should().BeNull();
            loaded10.Should().BeEquivalentTo(snapShot10);
            loaded100.Should().BeEquivalentTo(snapShot100);
        }

        private List<IRawStreamEntry> GetStreamEntries(string streamName, int count)
        {
            var emtpyDictionary = new Dictionary<string, object>();
            var emtpyDictionarySerialized = serializer.Serialize(emtpyDictionary);

            var entries = new List<IRawStreamEntry>();
            for (var i = 0; i < count; i++)
            {
                var entry = config.WriteModel.GenerateRawStreamEntry();
                entry.StreamName = streamName;
                entry.Headers = emtpyDictionarySerialized;
                entry.MessageId = $"messageId A {i}";
                entry.Payload = serializer.Serialize<object>(new TestEvent { Value = $"test A {i}" });
                entry.PayloadType = serializer.Binder.BindToName(typeof(TestEvent));
                entry.StreamRevision = 1;
                entry.DateTime = DateTime.UtcNow;

                entries.Add(entry);
            }
            return entries;
        }

        [Versioned("SnapshotTestState", 0)]
        public class SnapshotTestState : StreamState<SnapshotTestState>
        {
            public SnapshotTestState() { }
            public SnapshotTestState(string streamName)
            {
                StreamName = streamName;
            }

            public int TestEventCounter { get; private set; }

            public void Apply(TestEvent @event)
            {
                TestEventCounter++;
            }
        }
    }
}
