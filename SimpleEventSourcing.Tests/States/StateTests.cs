using FluentAssertions;
using NUnit.Framework;
using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.State;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleEventSourcing.Tests.States
{
    [TestFixture]
    public class StateTests
    {
        [Test]
        public void Can_invoke_event_or_message_handler_on_state()
        {
            var state = new TestState()
                .Invoke(new TestEvent().ToEventMessage())
                .Invoke(new TestEvent2());

            string.Join(", ",state.CalledHandlers)
                .Should().Be("IMessage<TestEvent>, TestEvent, TestEvent2");
        }

        public class TestState : EventSourcedState<TestState>
        {
            public List<string> CalledHandlers { get; set; } = new List<string>();

            public TestState Invoke(object eventOrMessage)
            {
                return InvokeAssociatedApply(eventOrMessage);
            }

            public void Apply(IMessage<TestEvent> e)
            {
                CalledHandlers.Add("IMessage<TestEvent>");
            }

            public void Apply(TestEvent e)
            {
                CalledHandlers.Add("TestEvent");
            }

            public void Apply(TestEvent2 e)
            {
                CalledHandlers.Add("TestEvent2");
            }
        }

        public class TestEvent : IEvent
        {

        }

        public class TestEvent2 : IEvent
        {

        }
    }
}
