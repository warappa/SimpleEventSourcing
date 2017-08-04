using SimpleEventSourcing.Messaging;
using System;

namespace SimpleEventSourcing.Tests
{
    public abstract class ChildBaseEvent : BaseEvent, IChildEntityEvent
    {
        public string AggregateRootId { get; private set; }

		object IChildEntityEvent.AggregateRootId => this.AggregateRootId;

		public ChildBaseEvent(string aggregateRootId, string id, DateTime dateTime)
            : base(id, dateTime)
        {
            this.AggregateRootId = aggregateRootId;
        }
    }
}