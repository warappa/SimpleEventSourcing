using FluentAssertions;
using NUnit.Framework;
using SimpleEventSourcing.Domain;
using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.State;
using SimpleEventSourcing.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleEventSourcing.WriteModel.Tests
{
    [TestFixture]
    public abstract class EventRepositoryTestsBase
    {
        protected TestsBaseConfig config;
        private EventRepository target;

        protected EventRepositoryTestsBase(TestsBaseConfig config)
        {
            this.config = config;
        }

        [SetUp]
        public void Setup()
        {
            target = new EventRepository(config.WriteModel.GetInstanceProvider(), config.WriteModel.GetPersistenceEngine(), config.WriteModel.GetRawStreamEntryFactory());
        }

        [TearDown]
        public void Teardown()
        {
            target?.Dispose();
        }

        [Test]
        public async Task Save_and_load_entity_with_child_entities()
        {
            var entity = new TestEntity(Guid.NewGuid().ToString(), "test");
            var child = entity.AddChild(Guid.NewGuid().ToString(), "child");
            child.Rename("child new name");
            await target.SaveAsync(entity);

            var loadedEntity = await target.GetAsync<TestEntity>(entity.Id);
            loadedEntity.Should().BeEquivalentTo(entity,x => x.ComparingByMembers<TestEntity>().ComparingByMembers<TestEntityState>().WithTracing());
            loadedEntity.StateModel.Name.Should().Be("test");
        }

        public class TestEntityState : AggregateRootState<TestEntityState, string>
        {
            public TestEntityState() : base()
		    {
                ChildStateCreationMap.Add(typeof(TestEntityChildAdded), evt => new TestChildEntityState((TestEntityChildAdded)evt));
            }

            public string Name { get; private set; }

            public IEnumerable<TestChildEntityState> Children => ChildStates.OfType<TestChildEntityState>().ToList();

            public void Apply(TestEntityCreated @event)
            {
                Id = @event.Id;
                Name = @event.Name;
            }

            public override bool Equals(object obj)
            {
                var other = obj as TestEntityState;
                if (other is null)
                {
                    return false;
                }

                return other.Id == Id &&
                    other.Name == Name;
            }

            public override int GetHashCode()
            {
                var hashCode = -1410341252;
                hashCode = hashCode * -1521134295 + base.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<Type[]>.Default.GetHashCode(PayloadTypes);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(StreamName);
                hashCode = hashCode * -1521134295 + EqualityComparer<IEnumerable<IChildEventSourcedState>>.Default.GetHashCode(ChildStates);
                hashCode = hashCode * -1521134295 + EqualityComparer<IDictionary<Type, Func<object, IChildEventSourcedState>>>.Default.GetHashCode(ChildStateCreationMap);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Id);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
                hashCode = hashCode * -1521134295 + EqualityComparer<IEnumerable<TestChildEntityState>>.Default.GetHashCode(Children);
                return hashCode;
            }

            public static bool operator ==(TestEntityState left, TestEntityState right)
            {
                return EqualityComparer<TestEntityState>.Default.Equals(left, right);
            }

            public static bool operator !=(TestEntityState left, TestEntityState right)
            {
                return !(left == right);
            }
        }

        public class TestChildEntityState : ChildEntityState<TestChildEntityState, string, string>
        {
            public TestChildEntityState():base()
            {

            }

            public TestChildEntityState(TestEntityChildAdded @event)
                :base()
            {
                Apply(@event);
            }

            public string Name { get; private set; }

            public void Apply(TestEntityChildAdded @event)
            {
                AggregateRootId = @event.AggregateRootId;
                Id = @event.Id;
                Name = @event.Name;
            }

            public void Apply(TestChildEntityRenamed @event)
            {
                Name = @event.Name;
            }

            public override bool Equals(object obj)
            {
                var other = obj as TestChildEntityState;
                if (other is null)
                {
                    return false;
                }

                return other.Id == Id &&
                    other.Name == Name;
            }

            public override int GetHashCode()
            {
                var hashCode = 530994897;
                hashCode = hashCode * -1521134295 + base.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<Type[]>.Default.GetHashCode(PayloadTypes);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(StreamName);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Id);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AggregateRootId);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
                return hashCode;
            }

            public static bool operator ==(TestChildEntityState left, TestChildEntityState right)
            {
                return EqualityComparer<TestChildEntityState>.Default.Equals(left, right);
            }

            public static bool operator !=(TestChildEntityState left, TestChildEntityState right)
            {
                return !(left == right);
            }
        }

        public class TestChildEntity : ChildEntity<TestChildEntityState, string, string>
        {
            public TestChildEntity(IAggregateRoot aggregateRoot, string id, string name)
                : base(aggregateRoot, new TestEntityChildAdded((string)aggregateRoot.Id, id, name))
            {

            }

            public void Rename(string newName)
            {
                RaiseEvent(new TestChildEntityRenamed(AggregateRootId, Id, newName));
            }

            public override bool Equals(object obj)
            {
                var other = obj as TestEntityChildAdded;
                if (other is null)
                {
                    return false;
                }

                return other.Id == Id;
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }
        }

        public class TestEntity : AggregateRoot<TestEntityState, string>
        {
            public TestEntity() : base(Enumerable.Empty<IEvent>()) { }
            public TestEntity(string id, string name)
                : base(new TestEntityCreated(id, name))
            {

            }

            public void Rename(string newName)
            {
                RaiseEvent(new TestEntityRenamed(Id, newName));
            }

            public TestChildEntity AddChild(string id, string name)
            {
                var newChild = new TestChildEntity(this, id, name);
                return newChild;
            }
        }

        public class TestEntityCreated : IEvent
        {
            public TestEntityCreated(string id, string name)
            {
                this.Id = id;
                this.Name = name;
            }

            public string Id { get; private set; }
            public string Name { get; private set; }
        }

        public class TestEntityRenamed : IEvent
        {
            public TestEntityRenamed(string id, string name)
            {
                this.Id = id;
                this.Name = name;
            }

            public string Id { get; private set; }
            public string Name { get; private set; }
        }

        public class TestEntityChildAdded : IChildEntityEvent
        {
            public TestEntityChildAdded(string aggregateRootId, string id, string name)
            {
                this.AggregateRootId = aggregateRootId;
                this.Id = id;
                this.Name = name;
            }

            public string AggregateRootId { get; private set; }
            public string Name { get; private set; }
            public string Id { get; private set; }

            object IChildEntityEvent.AggregateRootId => AggregateRootId;

            object IEventSourcedEntityEvent.Id => Id;
        }

        public class TestChildEntityRenamed : IChildEntityEvent
        {
            public TestChildEntityRenamed(string aggregateRootId, string id, string name)
            {
                this.AggregateRootId = aggregateRootId;
                this.Id = id;
                this.Name = name;
            }

            public string Id { get; private set; }
            public string AggregateRootId { get; private set; }
            public string Name { get; private set; }

            object IChildEntityEvent.AggregateRootId => AggregateRootId;

            object IEventSourcedEntityEvent.Id => Id;
        }
    }
}
