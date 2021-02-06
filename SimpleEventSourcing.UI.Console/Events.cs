using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.WriteModel;

namespace SimpleEventSourcing.UI.ConsoleUI
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
			Id = id;
			Name = name;
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
			Id = id;
			NewName = newName;
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
			Id = id;
			Bla = bla;
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
			Id = id;
			Bla = bla;
		}
	}
}
