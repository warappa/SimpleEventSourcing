using FluentAssertions;
using NUnit.Framework;
using SimpleEventSourcing.Domain;
using SimpleEventSourcing.Messaging;
using System;
using System.Linq;

namespace SimpleEventSourcing.Tests
{
    [TestFixture]
    public class ChildEntityTests
    {
        [Test]
        public void Ctor_with_argument_1_null_throws_ArgumentNullException()
        {
            ((Action)(() => new ChildEntity(null, Enumerable.Empty<IChildEntityEvent>()))).Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("aggregateRoot");
        }

        [Test]
        public void Ctor_with_argument_2_null_throws_ArgumentNullException()
        {
            ((Action)(() => new ChildEntity(new ParentEntity(Enumerable.Empty<IChildEntityEvent>()), null))).Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("events");
        }

        [Test]
        public void SetAggregateRoot_with_aggregateRoot_null_throws_ArgumentNullException()
        {
            var child = new ChildEntity();
            ((Action)(() => ((IChildEntityInternal)child).SetAggregateRoot(null, ChildEntityId.Generate()))).Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("aggregateRoot");
        }

        [Test]
        public void SetAggregateRoot_cannot_be_used_twice()
        {
            var child = new ChildEntity();
            ((IChildEntityInternal)child).SetAggregateRoot(new ParentEntity(ParentEntityId.Generate(), "name"), ChildEntityId.Generate());

            ((Action)(() => ((IChildEntityInternal)child).SetAggregateRoot(new ParentEntity(ParentEntityId.Generate(), "name"), ChildEntityId.Generate()))).Should().Throw<InvalidOperationException>()
                .And.Message.Should().Be("AggregateRoot cannot be reassigned in ChildEntity!");
        }

        [Test]
        public void Can_add()
        {
            var dateTime = DateTime.UtcNow;

            var parent = new ParentEntity(Guid.NewGuid().ToString(), "test parent");
            var child = new ChildEntity(parent, Guid.NewGuid().ToString(), "test child");
            child.Rename("test child renamed");

            parent.FixDateTime(dateTime);

            child.State.Id.Should().Be(child.Id);
            child.State.Name.Should().Be("test child renamed");
            var childStates = parent.State.ChildStates.ToList();
            childStates[0].Should().Be(child.State);
            (childStates[0] as ChildState).Name.Should().Be("test child renamed");

            var createdEvents = parent.AsIEventSourcedEntity().UncommittedEvents.ToList();

            createdEvents[0].Should().Be(new ParentCreated(parent.Id, "test parent", dateTime));
            createdEvents[1].Should().Be(new ChildCreated(parent.Id, child.Id, "test child", dateTime));
            createdEvents[2].Should().Be(new ChildRenamed(parent.Id, child.Id, "test child renamed", dateTime));

            var parent2 = new ParentEntity(createdEvents);
            (parent2.State.ChildStates.Single() as ChildState).Name.Should().Be("test child renamed");
            var child2 = new ChildEntity(parent2, createdEvents.Where(x => x is IChildEntityEvent).Cast<IChildEntityEvent>());
            (parent2 as IEventSourcedEntityInternal).RaiseEvent(new ChildRenamed(child.AggregateRootId, child.Id, "new new 3", dateTime));

            parent.State.StreamName.Should().Be(parent2.State.StreamName);
            parent.State.Name.Should().Be(parent2.State.Name);

            child2.State.Id.Should().Be(child.Id);
            child2.State.AggregateRootId.Should().Be(parent2.State.Id);
            child2.Id.Should().Be(child.Id);
            child2.State.Name.Should().Be("new new 3");
        }

        [Test]
        public void Can_add_child_entity_to_parent_entity()
        {
            var dateTime = DateTime.UtcNow;

            var parent = new ParentEntity(ParentEntityId.Generate(), "test parent");
            var child = new ChildEntity(parent, Guid.NewGuid().ToString(), "test child");
            child.Rename("test child renamed");

            parent.FixDateTime(dateTime);

            var createdEvents = parent.AsIEventSourcedEntity().UncommittedEvents.ToList();
            createdEvents[0].Should().Be(new ParentCreated(parent.Id, "test parent", dateTime));
            createdEvents[1].Should().Be(new ChildCreated(parent.Id, child.Id, "test child", dateTime));
            createdEvents[2].Should().Be(new ChildRenamed(parent.Id, child.Id, "test child renamed", dateTime));

            child.State.Id.Should().Be(child.Id);
            child.State.Name.Should().Be("test child renamed");

            var childStates = parent.State.ChildStates.ToList();

            childStates[0].Should().Be(child.State);
            (childStates[0] as ChildState).Name.Should().Be("test child renamed");
        }

        [Test]
        public void Can_add_child_entity_to_parent_entity_per_events()
        {
            var dateTime = DateTime.UtcNow;

            var parentId = "parent1";
            var childId = "child1";

            var parent = new ParentEntity(new IEvent[] {
                new ParentCreated(parentId, "test parent", dateTime),
                new ChildCreated(parentId, childId, "test child", dateTime),
                new ChildRenamed(parentId, childId, "test child renamed", dateTime)
            });

            parent.FixDateTime(dateTime);

            var child = parent.GetChildEntity<ChildEntity>(childId);
            child.State.Name.Should().Be("test child renamed");

            var childStates = parent.State.ChildStates.ToList();
            childStates[0].Should().Be(child.State);
            (childStates[0] as ChildState).Name.Should().Be("test child renamed");
        }

        [Test]
        public void Can_add_child_entity_to_parent_entity_per_initial_state()
        {
            var dateTime = DateTime.UtcNow;

            var parentId = "parent1";
            var childId = "child1";

            var parentState = new ParentState().Apply(new ParentCreated(parentId, "test parent", dateTime));

            var childState = new ChildState().Apply(new ChildCreated(parentId, childId, "test child", dateTime))
                .Apply(new ChildRenamed(parentId, childId, "test child renamed", dateTime));

            parentState.AsIAggregateRootStateInternal().AddChildState(childState);

            var parent = new ParentEntity(Array.Empty<IEvent>(), parentState);

            parent.FixDateTime(dateTime);

            var child = parent.GetChildEntity<ChildEntity>(childId);
            child.State.Name.Should().Be("test child renamed");

            var childStates = parent.State.ChildStates.ToList();
            childStates[0].Should().Be(child.State);
            (childStates[0] as ChildState).Name.Should().Be("test child renamed");
        }

        [Test]
        public void GetHashCode_should_be_the_state_hashcode()
        {
            var dateTime = DateTime.UtcNow;

            var parentId = "parent1";
            var childId = "child1";

            var parentState = new ParentState().Apply(new ParentCreated(parentId, "test parent", dateTime));

            var childState = new ChildState().Apply(new ChildCreated(parentId, childId, "test child", dateTime))
                .Apply(new ChildRenamed(parentId, childId, "test child renamed", dateTime));

            parentState.AsIAggregateRootStateInternal().AddChildState(childState);

            var parent = new ParentEntity(Array.Empty<IEvent>(), parentState);

            parent.FixDateTime(dateTime);

            var child = parent.GetChildEntity<ChildEntity>(childId);
            child.GetHashCode().Should().Be(child.State.GetHashCode());
        }

        [Test]
        public void Equals_should_call_equals_of_state()
        {
            var dateTime = DateTime.UtcNow;

            var parentId = "parent1";
            var childId = "child1";

            var parentState = new ParentState().Apply(new ParentCreated(parentId, "test parent", dateTime));

            var childState = new ChildState().Apply(new ChildCreated(parentId, childId, "test child", dateTime))
                .Apply(new ChildRenamed(parentId, childId, "test child renamed", dateTime));

            parentState.AsIAggregateRootStateInternal().AddChildState(childState);

            var parent = new ParentEntity(Array.Empty<IEvent>(), parentState);

            parent.FixDateTime(dateTime);

            var child = parent.GetChildEntity<ChildEntity>(childId);
            child.Equals(child.State).Should().Be(true);

            var child2 = parent.GetChildEntity<ChildEntity>(childId);

            ReferenceEquals(child, child2).Should().Be(false);

            child.Equals(child2).Should().Be(true);

            child.Equals(null).Should().Be(false);
        }
    }
}
