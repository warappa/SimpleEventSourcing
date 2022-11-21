using SimpleEventSourcing.State;
using SimpleEventSourcing.WriteModel;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SimpleEventSourcing.Tests.WriteModel
{
    [Versioned("TestChildEntityState", 0)]
    public class TestChildEntityState : ChildEntityState<TestChildEntityState, string, string>
    {
        public TestChildEntityState() : base()
        {

        }

        public TestChildEntityState(TestEntityChildAdded @event)
            : base()
        {
            Apply(@event);
        }

        [JsonInclude]
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
            hashCode = (hashCode * -1521134295) + base.GetHashCode();
            hashCode = (hashCode * -1521134295) + EqualityComparer<Type[]>.Default.GetHashCode(((IProjector)this).PayloadTypes);
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(StreamName);
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Id);
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(AggregateRootId);
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Name);
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
}
