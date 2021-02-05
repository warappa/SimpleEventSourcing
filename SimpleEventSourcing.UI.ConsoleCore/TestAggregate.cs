using SimpleEventSourcing.Domain;
using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.WriteModel;
using System.Linq;

namespace SimpleEventSourcing.UI.ConsoleCore
{
	[Versioned("TestAggregate", 0)]
	public class TestAggregate : AggregateRoot<TestState, string>
	{
		public TestAggregate() : base(Enumerable.Empty<IEvent>()) { }

		public TestAggregate(string id, string name)
			: base(new TestAggregateCreated(id, name))
		{

		}

		public void DoSomething(string bla)
		{
			RaiseEvent(new SomethingDone(this.stateModel.StreamName, bla));
		}

		public void DoSomethingSpecial(string bla)
		{
			RaiseEvent(new SomethingSpecialDone(this.stateModel.StreamName, bla));
		}

		internal void Rename(string name)
		{
			RaiseEvent(new Renamed(this.stateModel.StreamName, name));
		}
	}
}
