using SimpleEventSourcing.Domain;
using SimpleEventSourcing.Messaging;
using System.Linq;

namespace SimpleEventSourcing.WriteModel.Tests
{
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
            Id = id;
            Name = name;
        }

        public string Id { get; private set; }
        public string Name { get; private set; }
    }

    public class TestEntityRenamed : IEvent
    {
        public TestEntityRenamed(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; private set; }
        public string Name { get; private set; }
    }

    public class TestEntityChildAdded : IChildEntityEvent
    {
        public TestEntityChildAdded(string aggregateRootId, string id, string name)
        {
            AggregateRootId = aggregateRootId;
            Id = id;
            Name = name;
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
            AggregateRootId = aggregateRootId;
            Id = id;
            Name = name;
        }

        public string Id { get; private set; }
        public string AggregateRootId { get; private set; }
        public string Name { get; private set; }

        object IChildEntityEvent.AggregateRootId => AggregateRootId;

        object IEventSourcedEntityEvent.Id => Id;
    }
}
