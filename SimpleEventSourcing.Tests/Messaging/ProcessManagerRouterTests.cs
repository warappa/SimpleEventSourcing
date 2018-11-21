using NUnit.Framework;
using System;
using FakeItEasy;
using SimpleEventSourcing.Domain;
using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.WriteModel;
using FluentAssertions;
using SimpleEventSourcing.State;

namespace SimpleEventSourcing.Tests.Messaging
{
    [TestFixture]
    public class ProcessManagerRouterTests
    {
        private Func<IMessage, string> extractor;
        private IProcessManagerRepository processManagerRepository;
        private TestProcessManagerRouter target;

        [SetUp]
        public void Setup()
        {
            processManagerRepository = A.Fake<IProcessManagerRepository>();

            extractor = message => message.CorrelationId;

            target = new TestProcessManagerRouter(processManagerRepository, extractor);
        }

        [Test]
        public void Ctor_argument_1_is_null_should_throw_ArgumentNullException()
        {
            ((Action)(() => new TestProcessManagerRouter(null, null))).Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("processManagerRepository");
        }

        [Test]
        public void Ctor_argument_2_is_null_should_throw_ArgumentNullException()
        {
            ((Action)(() => new TestProcessManagerRouter(processManagerRepository, null))).Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("processIdExtractor");
        }

        [Test]
        public void Router_can_route_message_to_start_processmanager()
        {
            IProcessManager<TestProcessManagerState, string> processManager = null;

            

            var testEvent = new TestEvent("aggregateId", "test");
            var testMessage = new DummyMessage<TestEvent>("processId", testEvent);

            A.CallTo(() => processManagerRepository.Get(typeof(TestProcessManager), "processId")).ReturnsNextFromSequence(null, processManager);
            A.CallTo(() => processManagerRepository.Save(A<IProcessManager>._)).Invokes(x => processManager = (IProcessManager<TestProcessManagerState, string>)x.Arguments[0]);

            target.Register<TestProcessManager>();

            A.CallTo(() => processManagerRepository.Get(typeof(TestProcessManager), "processId")).MustNotHaveHappened();

            target.Handle(testMessage);

            processManager.StateModel.StreamName.Should().Be("processId");

            A.CallTo(() => processManagerRepository.Get(typeof(TestProcessManager), "processId")).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void Router_can_route_message_to_start_processmanager_until_it_ends()
        {
            IProcessManager<TestProcessManagerState, string> processManager = null;

            var testEvent = new TestEvent("aggregateId", "test");
            var testMessage = new DummyMessage<TestEvent>("processId", testEvent);

            A.CallTo(() => processManagerRepository.Get(typeof(TestProcessManager), "processId")).ReturnsLazily(() => processManager);
            A.CallTo(() => processManagerRepository.Save(A<IProcessManager>._)).Invokes(x => processManager = (IProcessManager<TestProcessManagerState, string>)x.Arguments[0]);

            target.Register<TestProcessManager>();

            A.CallTo(() => processManagerRepository.Get(typeof(TestProcessManager), "processId")).MustNotHaveHappened();

            target.Handle(testMessage);

            processManager.StateModel.StreamName.Should().Be("processId");

            A.CallTo(() => processManagerRepository.Get(typeof(TestProcessManager), "processId")).MustHaveHappened(Repeated.Exactly.Once);

            target.Handle(new DummyMessage<TestEventEnd>("processId", new TestEventEnd("anotherId")));

            processManager.StateModel.ProcessEnded.Should().Be(true);
        }

        [Test]
        public void Cannot_register_process_manager_with_no_start_events()
        {
            ((Action)(() => target.Register<ProcessManagerWithNoStartEvent>())).Should().Throw<InvalidOperationException>()
                .And.Message.Contains("has no start events").Should().Be(true);
        }

        [Test]
        public void Cannot_start_process_manager_with_events_other_than_start_events()
        {
            IProcessManager<TestProcessManagerState, string> processManager = null;
            
            var testMessage = new DummyMessage<TestEventEnd>("processId", new TestEventEnd("aggregateId"));

            A.CallTo(() => processManagerRepository.Get(typeof(TestProcessManager), "processId")).ReturnsLazily(() => processManager);
            A.CallTo(() => processManagerRepository.Save(A<IProcessManager>._)).Invokes(x => processManager = (IProcessManager<TestProcessManagerState, string>)x.Arguments[0]);

            target.Register<TestProcessManager>();

            ((Action)(() => target.Handle(testMessage))).Should().Throw<InvalidOperationException>()
                .And.Message.Contains("does not match any start events for").Should().Be(true);
        }

        public class ProcessManagerWithNoStartEvent : ProcessManager<ProcessManagerWithNoStartEventState, string>
        {
            public ProcessManagerWithNoStartEvent()
                : base(new IProcessManagerHandledEvent[0])
            {

            }
        }

        public class ProcessManagerWithNoStartEventState : ProcessManagerState<ProcessManagerWithNoStartEventState>
        {

        }
    }
}
