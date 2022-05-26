using SimpleEventSourcing.Domain;
using SimpleEventSourcing.Messaging;

namespace SimpleEventSourcing.Benchmarking.Domain
{
    public class TestAggregate : AggregateRoot<TestState, string>
    {
        public TestAggregate() : base(Enumerable.Empty<IEvent>()) { }

        public TestAggregate(string id, string name)
            : base(new TestAggregateCreated(id, name))
        {

        }

        public void DoSomething(string bla)
        {
            RaiseEvent(new SomethingDone(state.StreamName, bla));
        }

        public void DoSomethingSpecial(string bla)
        {
            RaiseEvent(new SomethingSpecialDone(state.StreamName, bla));
        }

        internal void Rename(string name)
        {
            RaiseEvent(new Renamed(state.StreamName, name));
        }
    }
}