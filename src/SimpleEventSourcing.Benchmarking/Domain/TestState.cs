using SimpleEventSourcing.State;
using SimpleEventSourcing.WriteModel;

namespace SimpleEventSourcing.Benchmarking.Domain
{
    [Versioned("TestState", 0)]
    public class TestState : AggregateRootState<TestState, string>
    {
        public string Name { get; protected set; }
        public string SomethingDone { get; protected set; }

        public TestState() : base() { }
        public TestState(TestState state)
            : base(state)
        {
            Name = state.Name;
            SomethingDone = state.SomethingDone;
        }

        public TestState Apply(TestAggregateCreated @event)
        {
            //Console.Write("C");
            // Console.Write(Environment.CurrentManagedThreadId);
            // Console.Write("|" + Environment.CurrentManagedThreadId);

            var s = new TestState(this)
            {
                StreamName = @event.Id,
                Name = @event.Name
            };

            return s;
        }

        public TestState Apply(SomethingDone @event)
        {
            //Console.Write("S");
            // Console.Write(Environment.CurrentManagedThreadId);

            // Console.Write("|" + Environment.CurrentManagedThreadId);

            var s = new TestState(this)
            {
                SomethingDone = @event.Bla
            };

            return s;
        }
        public TestState Apply(Renamed @event)
        {
            //Console.Write("R");
            // Console.Write(Environment.CurrentManagedThreadId);
            // Console.Write("|" + Environment.CurrentManagedThreadId);

            var s = new TestState(this)
            {
                Name = @event.Name
            };

            return s;
        }
    }
}
