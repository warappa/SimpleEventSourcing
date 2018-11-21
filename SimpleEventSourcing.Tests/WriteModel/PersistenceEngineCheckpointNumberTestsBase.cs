using FluentAssertions;
using NUnit.Framework;
using SimpleEventSourcing.Tests;
using System.Linq;

namespace SimpleEventSourcing.WriteModel.Tests
{
    [TestFixture]
    public abstract class PersistenceEngineCheckpointNumberTestsBase : PersistenceEngineTestsBase
    {
        protected PersistenceEngineCheckpointNumberTestsBase(TestsBaseConfig config)
            : base(config, true)
        {

        }

        [Test]
        public void Loading_unexistent_group_yields_zero_entries()
        {
            var loadedPerStream = persistenceEngine.LoadStreamEntries("nonexisting group", null).ToList();

            loadedPerStream.Should().HaveCount(0);
        }

        [Test]
        public void Can_load_all()
        {
            var loadedAll = persistenceEngine.LoadStreamEntries().ToList();

            var loaded = persistenceEngine.LoadStreamEntries(GroupConstants.All, null).ToList();

            loaded.Should().BeEquivalentTo(loadedAll);
        }

        [Test]
        public void Can_load_by_group_and_any_categories()
        {
            var loadedPerStream = persistenceEngine.LoadStreamEntries("testgroup", null).ToList();

            var expectedByStream = testEvents
                .WithGroup("testgroup");

            loadedPerStream.Should().BeEquivalentTo(expectedByStream);
        }

        [Test]
        public void Can_load_by_category_and_any_groups()
        {
            var loadedPerStream = persistenceEngine.LoadStreamEntries(GroupConstants.All, "testcategory").ToList();

            var expected = testEvents
                .WithCategory("testcategory");

            loadedPerStream.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Can_load_by_single_payloadType_accross_groups_and_categories()
        {
            var loaded = persistenceEngine.LoadStreamEntries(GroupConstants.All, null, payloadTypes: new[] { typeof(TestEvent2) }).ToList();

            var expected = testEvents
                .WithPayloadType<TestEvent2>();

            loaded.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Can_load_by_multiple_payloadTypes_accross_groups_and_categories()
        {
            var loaded = persistenceEngine.LoadStreamEntries(GroupConstants.All, null, payloadTypes: new[] { typeof(TestEvent), typeof(TestEvent2) }).ToList();

            var expected = testEvents
                .WithPayloadTypes<TestEvent, TestEvent2>();

            loaded.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Can_load_by_group_reverse()
        {
            var loaded = persistenceEngine.LoadStreamEntries("testgroup", null, ascending: false).ToList();

            var expected = testEvents
                .WithGroup("testgroup")
                .Reverse();

            loaded.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Can_load_by_checkpoint_number()
        {
            var loaded = persistenceEngine.LoadStreamEntries(1, 2).ToList();

            var expected = testEvents
                .Where(x => 
                    x.CheckpointNumber == 1 ||
                    x.CheckpointNumber == 2)
                .ToList();

            loaded.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Can_load_by_checkpoint_number_with_limit_respected()
        {
            var loaded = persistenceEngine.LoadStreamEntries(1, 3, take: 2).ToList();

            var expected = testEvents
                .Where(x =>
                    x.CheckpointNumber == 1 ||
                    x.CheckpointNumber == 2)
                .ToList();

            loaded.Should().BeEquivalentTo(expected);
        }
    }
}
