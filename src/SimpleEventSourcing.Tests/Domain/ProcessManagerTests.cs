using FluentAssertions;
using NUnit.Framework;
using SimpleEventSourcing.Domain;
using System.Linq;

namespace SimpleEventSourcing.Tests.Domain
{
    [TestFixture]
    public class ProcessManagerTests
    {
        [Test]
        public void ProcessManager_can_emit_command()
        {
            var processManager = new TestProcessManager();

            processManager.Handle("processId", new TestEvent("abcde", "name"));

            processManager.AsIProcessManager().UncommittedCommands.Count().Should().Be(1);

            processManager.Handle(new TestEventEnd("abc"));

            processManager.ProcessEnded.Should().Be(true);
        }

        [Test]
        public void ProcessManager_ends_on_end_event()
        {
            var processManager = new TestProcessManager();

            processManager.Handle("processId", new TestEvent("abcde", "name"));

            processManager.ProcessEnded.Should().BeFalse();

            processManager.Handle(new TestEventEnd("abc"));

            processManager.ProcessEnded.Should().Be(true);

            processManager.AsIProcessManager().UncommittedCommands.Count().Should().Be(1);
            processManager.AsIProcessManager().UncommittedEvents().Count().Should().Be(2);
        }

        [Test]
        public void ProcessManager_ignores_unknown_event()
        {
            var processManager = new TestProcessManager();

            processManager.Handle("processId", new TestEvent("abcde", "name"));
            processManager.AsIProcessManager().ClearUncommittedEvents();
            processManager.AsIProcessManager().ClearUncommittedCommands();

            processManager.ProcessEnded.Should().BeFalse();

            ((IProcessManager)processManager).Handle(new TestEventUnknown("abc"));

            processManager.AsIProcessManager().UncommittedCommands.Count().Should().Be(0);
            processManager.AsIProcessManager().UncommittedEvents().Count().Should().Be(0);

            processManager.ProcessEnded.Should().Be(false);
        }
    }
}
