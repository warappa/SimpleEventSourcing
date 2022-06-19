using SimpleEventSourcing.State;

namespace SimpleEventSourcing.Tests
{
    public class TestProcessManagerState : ProcessManagerState<TestProcessManagerState>
    {
        public string ProcessName { get; set; }

        public TestProcessManagerState Apply(TestEvent @event)
        {
            ProcessName = @event.Value;

            return this;
        }

        public TestProcessManagerState Apply(TestEventEnd @event)
        {
            ProcessEnded = true;
            return this;
        }
    }
}
