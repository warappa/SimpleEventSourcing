using SimpleEventSourcing.Messaging;
using System;

namespace SimpleEventSourcing.Tests.Domain.TestData
{
    public abstract class ChildBaseEvent : BaseEvent, IChildEntityEvent
    {
        public string AggregateRootId { get; private set; }

        object IChildEntityEvent.AggregateRootId => AggregateRootId;

        protected ChildBaseEvent(string aggregateRootId, string id, DateTime dateTime)
            : base(id, dateTime)
        {
            AggregateRootId = aggregateRootId;
        }
    }
}
