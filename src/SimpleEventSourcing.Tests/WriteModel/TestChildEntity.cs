using SimpleEventSourcing.Domain;

namespace SimpleEventSourcing.WriteModel.Tests
{
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
}
