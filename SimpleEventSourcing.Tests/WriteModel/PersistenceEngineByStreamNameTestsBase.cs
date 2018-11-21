using FluentAssertions;
using NUnit.Framework;
using SimpleEventSourcing.Tests;
using System.Linq;

namespace SimpleEventSourcing.WriteModel.Tests
{
    [TestFixture]
    public abstract class PersistenceEngineByStreamNameTestsBase : PersistenceEngineTestsBase
    {
        public PersistenceEngineByStreamNameTestsBase(TestsBaseConfig config)
            : base(config, true)
        {

        }

        [Test]
        public void Loading_unexistent_stream_yields_zero_entries()
        {
            var loaded = persistenceEngine.LoadStreamEntriesByStream(GroupConstants.All, null, "teststream nonexistent").ToList();

            loaded.Should().HaveCount(0);
        }

        [Test]
        public void Can_load_by_streamName_accross_groups_and_categories()
        {
            var loaded = persistenceEngine.LoadStreamEntriesByStream(GroupConstants.All, null, "teststream A").ToList();

            var expected = testEvents.WithStreamName("teststream A");

            loaded.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Can_load_by_streamName_and_group_and_any_categories()
        {
            var loaded = persistenceEngine.LoadStreamEntriesByStream("testgroup", null, "teststream A").ToList();

            var expected = testEvents
                .WithStreamName("teststream A")
                .WithGroup("testgroup");

            loaded.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Can_load_by_streamName_and_category_and_any_groups()
        {
            var loaded = persistenceEngine.LoadStreamEntriesByStream(GroupConstants.All, "testcategory", "teststream A").ToList();

            var expected = testEvents
                .WithStreamName("teststream A")
                .WithCategory("testcategory")
                .ToList();

            loaded.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Can_load_by_streamName_and_single_payloadType_accross_groups_and_categories()
        {
            var loaded = persistenceEngine.LoadStreamEntriesByStream(GroupConstants.All, null, "teststream A", payloadTypes: new[] { typeof(TestEvent2) }).ToList();

            var expected = testEvents
                .WithStreamName("teststream A")
                .WithPayloadType<TestEvent2>();

            loaded.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Can_load_by_streamName_and_multiple_payloadTypes_accross_groups_and_categories()
        {
            var loaded = persistenceEngine.LoadStreamEntriesByStream(GroupConstants.All, null, "teststream A", payloadTypes: new[] { typeof(TestEvent), typeof(TestEvent2) }).ToList();

            var expected = testEvents
                .WithStreamName("teststream A")
                .WithPayloadTypes<TestEvent, TestEvent2>();

            loaded.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Can_load_by_streamName_reverse()
        {
            var loaded = persistenceEngine.LoadStreamEntriesByStream(GroupConstants.All, null, "teststream A", ascending: false).ToList();

            var expected = testEvents
                .WithStreamName("teststream A")
                .Reverse();

            loaded.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Can_load_by_stream_revision()
        {
            var loaded = persistenceEngine.LoadStreamEntriesByStream("teststream A", 1, 2).ToList();

            var expected = testEvents
                .WithStreamName("teststream A")
                .Where(x =>
                    x.StreamRevision == 1 ||
                    x.StreamRevision == 2)
                .ToList();

            loaded.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Can_load_by_checkpoint_number_with_take_respected()
        {
            var loaded = persistenceEngine.LoadStreamEntriesByStream("teststream A", 1, 3, take: 2).ToList();

            var expected = testEvents
                .WithGroup(null)
                .WithCategory(null)
                .WithStreamName("teststream A")
                .Where(x =>
                    x.StreamRevision == 1 ||
                    x.StreamRevision == 2)
                .ToList();

            loaded.Should().BeEquivalentTo(expected);
        }
    }
}
