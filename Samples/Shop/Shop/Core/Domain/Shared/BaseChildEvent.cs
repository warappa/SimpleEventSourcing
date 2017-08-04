using SimpleEventSourcing.Messaging;
using System;

namespace Shop.Core.Domain.Shared
{
    public abstract class BaseChildEvent : IChildEntityEvent
    {
        public object AggregateRootId { get; private set; }
        public DateTime DateTime { get; private set; }
        public object Id { get; private set; }

        public BaseChildEvent(string aggregateRootId, string id, DateTime dateTime)
        {
            AggregateRootId = aggregateRootId;
            Id = id;
            DateTime = dateTime;
        }
    }
}
