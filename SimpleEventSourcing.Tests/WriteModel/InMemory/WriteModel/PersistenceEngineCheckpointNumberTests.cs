using FluentAssertions;
using NUnit.Framework;
using SimpleEventSourcing.WriteModel.InMemory.Tests;
using SimpleEventSourcing.WriteModel.Tests;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleEventSourcing.WriteModel.InMemory.WriteModel.Tests
{
    [TestFixture]
    public abstract class PersistenceEngineCheckpointNumberTestsInMemory : PersistenceEngineTestsBase
    {
        public PersistenceEngineCheckpointNumberTestsInMemory()
            : base(new InMemoryTestConfig())
        {

        }

        [Test]
        public async Task Loading_unexistent_group_yields_zero_entries()
        {
            var loadedPerStream = await persistenceEngine.LoadStreamEntriesAsync("nonexisting group", null)
                .ToListAsync();

            loadedPerStream.Should().HaveCount(0);
        }

        [Test]
        public async Task Can_load_all()
        {
            var loadedAll = await persistenceEngine.LoadStreamEntriesAsync()
                .ToListAsync();

            var loaded = await persistenceEngine.LoadStreamEntriesAsync(GroupConstants.All, null)
                .ToListAsync();

            loaded.Should().BeEquivalentTo(loadedAll);
        }

        [Test]
        public async Task Can_load_by_group_and_any_categories()
        {
            var loadedPerStream = await persistenceEngine.LoadStreamEntriesAsync("testgroup", null)
                .ToListAsync();

            var expectedByStream = testEvents
                .WithGroup("testgroup");

            loadedPerStream.Should().BeEquivalentTo(expectedByStream);
        }

        [Test]
        public async Task Can_load_by_category_and_any_groups()
        {
            var loadedPerStream = await persistenceEngine.LoadStreamEntriesAsync(GroupConstants.All, "testcategory")
                .ToListAsync();

            var expected = testEvents
                .WithCategory("testcategory");

            loadedPerStream.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task Can_load_by_single_payloadType_accross_groups_and_categories()
        {
            var loaded = await persistenceEngine.LoadStreamEntriesAsync(GroupConstants.All, null, payloadTypes: new[] { typeof(TestEvent2) })
                .ToListAsync();

            var expected = testEvents
                .WithPayloadType<TestEvent2>();

            loaded.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task Can_load_by_multiple_payloadTypes_accross_groups_and_categories()
        {
            var loaded = await persistenceEngine.LoadStreamEntriesAsync(GroupConstants.All, null, payloadTypes: new[] { typeof(TestEvent), typeof(TestEvent2) })
                .ToListAsync();

            var expected = testEvents
                .WithPayloadTypes<TestEvent, TestEvent2>();

            loaded.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task Can_load_by_group_reverse()
        {
            var loaded = await persistenceEngine.LoadStreamEntriesAsync("testgroup", null, ascending: false)
                .ToListAsync();

            var expected = testEvents
                .WithGroup("testgroup")
                .Reverse();

            loaded.Should().BeEquivalentTo(expected);
        }
    }
}
