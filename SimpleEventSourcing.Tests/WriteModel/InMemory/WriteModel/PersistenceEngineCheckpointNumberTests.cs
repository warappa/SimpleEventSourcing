using FluentAssertions;
using NUnit.Framework;
using SimpleEventSourcing.WriteModel.InMemory.Tests;
using SimpleEventSourcing.WriteModel.Tests;
using System.Linq;

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

            loaded.ShouldBeEquivalentTo(loadedAll);
        }

        [Test]
        public void Can_load_by_group_and_any_categories()
        {
            var loadedPerStream = persistenceEngine.LoadStreamEntries("testgroup", null).ToList();

            var expectedByStream = testEvents
                .WithGroup("testgroup");

            loadedPerStream.ShouldBeEquivalentTo(expectedByStream);
        }

        [Test]
        public void Can_load_by_category_and_any_groups()
        {
            var loadedPerStream = persistenceEngine.LoadStreamEntries(GroupConstants.All, "testcategory").ToList();

            var expected = testEvents
                .WithCategory("testcategory");

            loadedPerStream.ShouldBeEquivalentTo(expected);
        }

        [Test]
        public void Can_load_by_single_payloadType_accross_groups_and_categories()
        {
            var loaded = persistenceEngine.LoadStreamEntries(GroupConstants.All, null, payloadTypes: new[] { typeof(TestEvent2) }).ToList();

            var expected = testEvents
                .WithPayloadType<TestEvent2>();

            loaded.ShouldBeEquivalentTo(expected);
        }

        [Test]
        public void Can_load_by_multiple_payloadTypes_accross_groups_and_categories()
        {
            var loaded = persistenceEngine.LoadStreamEntries(GroupConstants.All, null, payloadTypes: new[] { typeof(TestEvent), typeof(TestEvent2) }).ToList();

            var expected = testEvents
                .WithPayloadTypes<TestEvent, TestEvent2>();

            loaded.ShouldBeEquivalentTo(expected);
        }

        [Test]
        public void Can_load_by_group_reverse()
        {
            var loaded = persistenceEngine.LoadStreamEntries("testgroup", null, ascending: false).ToList();

            var expected = testEvents
                .WithGroup("testgroup")
                .Reverse();

            loaded.ShouldBeEquivalentTo(expected);
        }
    }
}
