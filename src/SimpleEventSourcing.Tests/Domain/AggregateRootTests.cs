using FluentAssertions;
using NUnit.Framework;
using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.Tests.Domain.TestData;
using System;
using System.Linq;

namespace SimpleEventSourcing.Tests.Domain
{
    [TestFixture]
    public class AggregateRootTests
    {
        [Test]
        public void Entity_updates_State()
        {
            var id = ParentEntityId.Generate();

            var parent = new ParentEntity(id, "test parent");

            parent.State.Id.Should().Be(id);
            parent.State.Name.Should().Be("test parent");
        }

        [Test]
        public void Entity_updates_State_by_events()
        {
            var id = ParentEntityId.Generate();
            var parent = new ParentEntity(new ParentCreated(id, "test parent", DateTime.UtcNow));

            parent.State.Id.Should().Be(id);
            parent.State.Name.Should().Be("test parent");
        }

        [Test]
        public void Entity_updates_State_by_inital_state()
        {
            var id = ParentEntityId.Generate();
            var state = new ParentState().Apply(new ParentCreated(id, "test parent", DateTime.UtcNow));

            var parent = new ParentEntity(Enumerable.Empty<IEvent>(), state);

            parent.State.Id.Should().Be(id);
            parent.State.Name.Should().Be("test parent");
        }

        [Test]
        public void GetHashCode_should_be_the_state_hashcode()
        {
            var id = ParentEntityId.Generate();
            var state = new ParentState().Apply(new ParentCreated(id, "test parent", DateTime.UtcNow));

            var parent = new ParentEntity(Enumerable.Empty<IEvent>(), state);

            parent.GetHashCode().Should().Be(parent.State.GetHashCode());
        }

        [Test]
        public void Equals_should_call_equals_of_state()
        {
            var id = ParentEntityId.Generate();
            var state = new ParentState().Apply(new ParentCreated(id, "test parent", DateTime.UtcNow));

            var parent = new ParentEntity(Enumerable.Empty<IEvent>(), state);

            parent.Equals(parent.State).Should().Be(true);

            var state2 = new ParentState().Apply(new ParentCreated(id, "test parent", DateTime.UtcNow));
            var parent2 = new ParentEntity(Enumerable.Empty<IEvent>(), state2);
            parent.Equals(parent2).Should().Be(true);

            parent.Equals(null).Should().Be(false);
        }
    }
}
