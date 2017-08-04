using FluentAssertions;
using NUnit.Framework;
using SimpleEventSourcing.Domain;
using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.State;
using System;
using System.Linq;

namespace SimpleEventSourcing.Tests.Domain
{
    [TestFixture]
    public class AggregateRootTests
    {
        [Test]
        public void Entity_updates_StateModel()
        {
            var id = ParentEntityId.Generate();

            var parent = new ParentEntity(id, "test parent");

            parent.StateModel.Id.Should().Be(id);
            parent.StateModel.Name.Should().Be("test parent");
        }

        [Test]
        public void Entity_updates_StateModel_by_events()
        {
            var id = ParentEntityId.Generate();
            var parent = new ParentEntity(new ParentCreated(id, "test parent", DateTime.UtcNow));

            parent.StateModel.Id.Should().Be(id);
            parent.StateModel.Name.Should().Be("test parent");
        }

        [Test]
        public void Entity_updates_StateModel_by_inital_state()
        {
            var id = ParentEntityId.Generate();
            var state = new ParentState().Apply(new ParentCreated(id, "test parent", DateTime.UtcNow));

            var parent = new ParentEntity(Enumerable.Empty<IEvent>(), state);

            parent.StateModel.Id.Should().Be(id);
            parent.StateModel.Name.Should().Be("test parent");
        }

        [Test]
        public void GetHashCode_should_be_the_state_hashcode()
        {
            var id = ParentEntityId.Generate();
            var state = new ParentState().Apply(new ParentCreated(id, "test parent", DateTime.UtcNow));

            var parent = new ParentEntity(Enumerable.Empty<IEvent>(), state);

            parent.GetHashCode().Should().Be(parent.StateModel.GetHashCode());
        }

        [Test]
        public void Equals_should_call_equals_of_state()
        {
            var id = ParentEntityId.Generate();
            var state = new ParentState().Apply(new ParentCreated(id, "test parent", DateTime.UtcNow));

            var parent = new ParentEntity(Enumerable.Empty<IEvent>(), state);

            parent.Equals(parent.StateModel).Should().Be(true);

            var state2 = new ParentState().Apply(new ParentCreated(id, "test parent", DateTime.UtcNow));
            var parent2 = new ParentEntity(Enumerable.Empty<IEvent>(), state2);
            parent.Equals(parent2).Should().Be(true);

            parent.Equals(null).Should().Be(false);
        }
    }
}
