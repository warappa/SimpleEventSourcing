using NUnit.Framework;
using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.ReadModel;
using SimpleEventSourcing.State;

namespace SimpleEventSourcing.Tests.States
{

	[TestFixture]
	public class TestClass
	{
		[Test]
		public void Test()
		{
			new TestState();
			new TestState2();
		}
	}
	public class TestState : ReadRepositoryState<TestState>
	{
		public TestState()
			: base(null)
		{

		}
		public void Apply(TestEvent e)
		{

		}
	}

	public class TestState2 : EventSourcedState<TestState2>
	{
		public void Apply(IMessage<TestEvent> e)
		{

		}
	}

	public class TestEvent : IEvent
	{

	}
}
