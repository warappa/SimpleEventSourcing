using SimpleEventSourcing.State;
using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.UI.ConsoleUI
{
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

        public Task<TestState> Apply(TestAggregateCreated @event)
        {
            Console.Write("C");
            // Console.Write(Environment.CurrentManagedThreadId);
            // Console.Write("|" + Environment.CurrentManagedThreadId);

            var s = new TestState(this);
            s.StreamName = @event.Id;
            s.Name = @event.Name;

            return Task.FromResult(s);
        }

        public Task<TestState> Apply(SomethingDone @event)
        {
            Console.Write("S");
            // Console.Write(Environment.CurrentManagedThreadId);
            
            // Console.Write("|" + Environment.CurrentManagedThreadId);

            var s = new TestState(this);
            s.SomethingDone = @event.Bla;

            return Task.FromResult(s);
        }
        public Task<TestState> Apply(Renamed @event)
        {
            Console.Write("R");
            // Console.Write(Environment.CurrentManagedThreadId);
            // Console.Write("|" + Environment.CurrentManagedThreadId);

            var s = new TestState(this);
            s.Name = @event.Name;

            return Task.FromResult(s);
        }
    }
}
