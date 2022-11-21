using SimpleEventSourcing.Domain;
using System;

namespace SimpleEventSourcing.Tests.Domain.TestData
{
    public class TestProcessManager : ProcessManager<TestProcessManagerState, string>,
        IProcessManagerStartsWith<TestEvent>,
        IProcessManagerHandles<TestEventEnd>
    {
        public TestProcessManager()
            : base(Array.Empty<IProcessManagerHandledEvent>())
        {

        }

        public void Handle(string processId, TestEvent @event)
        {
            HandledStartEvent(processId, @event);

            SendCommand(new TestCommandsOne("agg2", "testcommand"));
        }

        public void Handle(TestEventEnd @event)
        {
            HandledEvent(@event);
        }
    }
}
