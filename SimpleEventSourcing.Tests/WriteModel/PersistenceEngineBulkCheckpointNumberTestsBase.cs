using FluentAssertions;
using NUnit.Framework;
using SimpleEventSourcing.Tests;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleEventSourcing.WriteModel.Tests
{
    [TestFixture]
    public abstract class PersistenceEngineBulkCheckpointNumberTestsBase : PersistenceEngineTestsBase
    {
        protected PersistenceEngineBulkCheckpointNumberTestsBase(TestsBaseConfig config)
            : base(config, false)
        {

        }

        [SetUp]
        public async Task TestSetupAsync()
        {
            EarlySetup();

            await InitializeAsync();
        }

        [Test]
        public async Task Entities_are_in_the_same_order_as_they_were_inserted()
        {
            const int sampleSize = 1000;

            var entries = new List<IRawStreamEntry>();
            for (var i = 0; i < sampleSize; i++)
            {
                var entry = config.WriteModel.GenerateRawStreamEntry();
                entry.StreamRevision = i + 1;
                entries.Add(entry);
            }

            await persistenceEngine.SaveStreamEntriesAsync(entries);

            // patch for comparison
            for (var i = 0; i < sampleSize; i++)
            {
                entries[i].CheckpointNumber = i + 1;
            }

            var loaded = await persistenceEngine.LoadStreamEntriesAsync()
                .ToListAsync();

            var actual = loaded.Select(x => x.StreamRevision).ToArray();
            var expected = entries.Select(x => x.StreamRevision).ToArray();

            actual.Should().BeEquivalentTo(expected/*, x=> x.ComparingByValue<int>()*/, "aaa");
        }

    }
}
