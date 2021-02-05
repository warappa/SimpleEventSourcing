using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.WriteModel;

namespace SimpleEventSourcing.UI.ConsoleCore
{
	[Versioned("TestAggregateCreated", 0)]
	public class TestAggregateCreated : IEvent, INameChangeEvent
	{
		public string Id;
		public string Name;

		public TestAggregateCreated(
			string id,
			string name)
		{
			this.Id = id;
			this.Name = name;
		}

		string INameChangeEvent.Name
		{
			get { return Name; }
		}
	}

	[Versioned("Renamed", 0)]
	public class Renamed : IEvent, INameChangeEvent
	{
		public string Id;
		public string NewName;

		public Renamed(
			string id,
			string newName)
		{
			this.Id = id;
			this.NewName = newName;
		}

		public string Name
		{
			get { return NewName; }
		}
	}

	[Versioned("SomethingDone", 0)]
	public class SomethingDone : IEvent
	{
		public string Id;
		public string Bla;

		public SomethingDone(
			string id,
			string bla)
		{
			this.Id = id;
			this.Bla = bla;
		}
	}
	[Versioned("SomethingSpecialDone", 0)]
	public class SomethingSpecialDone : IEvent
	{
		public string Id;
		public string Bla;

		public SomethingSpecialDone(
			string id,
			string bla)
		{
			this.Id = id;
			this.Bla = bla;
		}
	}
}
