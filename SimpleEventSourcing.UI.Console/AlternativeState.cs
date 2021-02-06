using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.State;
using System.Diagnostics;

namespace SimpleEventSourcing.UI.ConsoleUI
{
    public class AlternativeState : EventSourcedState<AlternativeState>
	{
		public int ChangeCount { get; protected set; }

		public AlternativeState Apply(IEvent @event)
		{
			Debug.WriteLine("Apply every event");

			ChangeCount++;

			return this;
		}
	}
}
