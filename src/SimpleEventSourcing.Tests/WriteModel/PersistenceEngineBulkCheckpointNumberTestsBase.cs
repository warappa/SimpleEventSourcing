using FluentAssertions;
using NUnit.Framework;
using SimpleEventSourcing.WriteModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleEventSourcing.Tests.WriteModel
{
    [TestFixture]
    public abstract class PersistenceEngineBulkCheckpointNumberTestsBase : PersistenceEngineTestsBase
    {
        protected PersistenceEngineBulkCheckpointNumberTestsBase(TestsBaseConfig config)
            : base(config, false)
        {

        }

        [SetUp]
        public new async Task TestSetupAsync()
        {
            EarlySetup();

            await InitializeAsync().ConfigureAwait(false);
        }

        [Test]
        public async Task Entities_are_in_the_same_order_as_they_were_inserted_checked_by_StreamRevision()
        {
            const int sampleSize = 50000;

            var entries = new List<IRawStreamEntry>();
            for (var i = 0; i < sampleSize; i++)
            {
                var entry = config.WriteModel.GenerateRawStreamEntry();
                entry.StreamRevision = i + 1;
                entries.Add(entry);
            }

            await persistenceEngine.SaveStreamEntriesAsync(entries).ConfigureAwait(false);

            // patch for comparison
            for (var i = 0; i < sampleSize; i++)
            {
                entries[i].CheckpointNumber = i + 1;
            }

            var loaded = await persistenceEngine.LoadStreamEntriesAsync()
                .ToListAsync()
                .ConfigureAwait(false);

            var actual = loaded.Select(x => x.StreamRevision).ToArray();
            var expected = entries.Select(x => x.StreamRevision).ToArray();

            actual.Should().BeEquivalentTo(expected, "StreamRevision order not preserved");
        }

        [Test]
        public virtual async Task Entities_are_in_the_same_order_as_they_were_inserted_checked_by_checkpointnumber()
        {
            const int sampleSize = 50000;

            var entries = new List<IRawStreamEntry>();
            for (var i = 0; i < sampleSize; i++)
            {
                var entry = config.WriteModel.GenerateRawStreamEntry();
                entry.StreamRevision = i + 1;
                entries.Add(entry);
            }

            Console.WriteLine("start");
            var stopwatch = new Stopwatch();

            stopwatch.Start();
            await persistenceEngine.SaveStreamEntriesAsync(entries).ConfigureAwait(false);
            stopwatch.Stop();
            Console.WriteLine($"\nInsert Duration: {stopwatch.ElapsedMilliseconds}ms\n");

            // patch for comparison
            for (var i = 0; i < sampleSize; i++)
            {
                entries[i].CheckpointNumber = i + 1;
            }

            var loaded = await persistenceEngine.LoadStreamEntriesAsync()
                .ToListAsync()
                .ConfigureAwait(false);

            var actual = loaded.Select(x => x.CheckpointNumber).ToArray();
            var expected = entries.Select(x => x.CheckpointNumber).ToArray();

            actual.Should().BeEquivalentTo(expected, "Checkpoint order not preserved");
        }
    }
}
