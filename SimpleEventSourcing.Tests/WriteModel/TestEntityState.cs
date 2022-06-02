using SimpleEventSourcing.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SimpleEventSourcing.WriteModel.Tests
{
    public class TestEntityState : AggregateRootState<TestEntityState, string>
    {
        static TestEntityState()
        {
            childStateCreationMap.Add(typeof(TestEntityChildAdded), evt => new TestChildEntityState((TestEntityChildAdded)evt));
        }

        [JsonInclude]
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
            hashCode = hashCode * -1521134295 + EqualityComparer<Type[]>.Default.GetHashCode(((IProjector)this).PayloadTypes);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(StreamName);
            hashCode = hashCode * -1521134295 + EqualityComparer<IEnumerable<IChildEventSourcedState>>.Default.GetHashCode(ChildStates);
            hashCode = hashCode * -1521134295 + EqualityComparer<IDictionary<Type, Func<object, IChildEventSourcedState>>>.Default.GetHashCode(childStateCreationMap);
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
}
