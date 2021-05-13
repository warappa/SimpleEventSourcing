using SimpleEventSourcing.State;
using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.UI.ConsoleCore
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

        public Task<TestState> ApplyAsync(TestAggregateCreated @event)
        {
            Console.Write("C");
            // Console.Write(Environment.CurrentManagedThreadId);
            // Console.Write("|" + Environment.CurrentManagedThreadId);

            var s = new TestState(this)
            {
                StreamName = @event.Id,
                Name = @event.Name
            };

            return Task.FromResult(s);
        }

        public TestState ApplyAsync(SomethingDone @event)
        {
            Console.Write("S");
            // Console.Write(Environment.CurrentManagedThreadId);

            // Console.Write("|" + Environment.CurrentManagedThreadId);

            var s = new TestState(this)
            {
                SomethingDone = @event.Bla
            };

            return s;
        }
        public Task<TestState> ApplyAsync(Renamed @event)
        {
            Console.Write("R");
            // Console.Write(Environment.CurrentManagedThreadId);
            // Console.Write("|" + Environment.CurrentManagedThreadId);

            var s = new TestState(this)
            {
                Name = @event.Name
            };

            return Task.FromResult(s);
        }
    }
}
