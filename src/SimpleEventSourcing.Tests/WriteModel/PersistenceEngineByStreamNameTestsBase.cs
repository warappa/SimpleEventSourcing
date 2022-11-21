using FluentAssertions;
using NUnit.Framework;
using SimpleEventSourcing.WriteModel;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleEventSourcing.Tests.WriteModel
{
    [TestFixture]
    public abstract class PersistenceEngineByStreamNameTestsBase : PersistenceEngineTestsBase
    {
        protected PersistenceEngineByStreamNameTestsBase(TestsBaseConfig config)
            : base(config, true)
        {

        }

        [Test]
        public async Task Loading_unexistent_stream_yields_zero_entries()
        {
            var loaded = await persistenceEngine.LoadStreamEntriesByStreamAsync(GroupConstants.All, null, "teststream nonexistent")
                .ToListAsync()
                .ConfigureAwait(false);

            loaded.Should().HaveCount(0);
        }

        [Test]
        public async Task Can_load_by_streamName_accross_groups_and_categories()
        {
            var loaded = await persistenceEngine.LoadStreamEntriesByStreamAsync(GroupConstants.All, null, "teststream A")
                .ToListAsync()
                .ConfigureAwait(false);

            var expected = testEvents
                .WithStreamName("teststream A");

            loaded.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task Can_load_by_streamName_and_group_and_any_categories()
        {
            var loaded = await persistenceEngine.LoadStreamEntriesByStreamAsync("testgroup", null, "teststream A")
                .ToListAsync()
                .ConfigureAwait(false);

            var expected = testEvents
                .WithStreamName("teststream A")
                .WithGroup("testgroup");

            loaded.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task Can_load_by_streamName_and_category_and_any_groups()
        {
            var loaded = await persistenceEngine.LoadStreamEntriesByStreamAsync(GroupConstants.All, "testcategory", "teststream A")
                .ToListAsync()
                .ConfigureAwait(false);

            var expected = testEvents
                .WithStreamName("teststream A")
                .WithCategory("testcategory")
                .ToList();

            loaded.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task Can_load_by_streamName_and_single_payloadType_accross_groups_and_categories()
        {
            var loaded = await persistenceEngine.LoadStreamEntriesByStreamAsync(GroupConstants.All, null, "teststream A", payloadTypes: new[] { typeof(TestEvent2) })
                .ToListAsync()
                .ConfigureAwait(false);

            var expected = testEvents
                .WithStreamName("teststream A")
                .WithPayloadType<TestEvent2>();

            loaded.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task Can_load_by_streamName_and_multiple_payloadTypes_accross_groups_and_categories()
        {
            var loaded = await persistenceEngine.LoadStreamEntriesByStreamAsync(GroupConstants.All, null, "teststream A", payloadTypes: new[] { typeof(TestEvent), typeof(TestEvent2) })
                .ToListAsync()
                .ConfigureAwait(false);

            var expected = testEvents
                .WithStreamName("teststream A")
                .WithPayloadTypes<TestEvent, TestEvent2>();

            loaded.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task Can_load_by_streamName_reverse()
        {
            var loaded = await persistenceEngine.LoadStreamEntriesByStreamAsync(GroupConstants.All, null, "teststream A", ascending: false)
                .ToListAsync()
                .ConfigureAwait(false);

            var expected = testEvents
                .WithStreamName("teststream A")
                .Reverse();

            loaded.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task Can_load_by_stream_revision()
        {
            var loaded = await persistenceEngine.LoadStreamEntriesByStreamAsync("teststream A", 1, 2)
                .ToListAsync()
                .ConfigureAwait(false);

            var expected = testEvents
                .WithStreamName("teststream A")
                .Where(x =>
                    x.StreamRevision is 1 or
                    2)
                .ToList();

            loaded.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task Can_load_by_checkpoint_number_with_take_respected()
        {
            var loaded = await persistenceEngine.LoadStreamEntriesByStreamAsync("teststream A", 1, 3, take: 2)
                .ToListAsync()
                .ConfigureAwait(false);

            var expected = testEvents
                .WithGroup(null)
                .WithCategory(null)
                .WithStreamName("teststream A")
                .Where(x =>
                    x.StreamRevision is 1 or
                    2)
                .ToList();

            loaded.Should().BeEquivalentTo(expected);
        }
    }
}
