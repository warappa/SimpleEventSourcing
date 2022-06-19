using SimpleEventSourcing.Messaging;
using System;

namespace Shop.Core.Domain.Shared
{
    public abstract class BaseEvent : IEvent
    {
        public DateTime DateTime { get; private set; }
        public string Id { get; private set; }

        protected BaseEvent(string id, DateTime dateTime)
        {
            Id = id;
            DateTime = dateTime;
        }
    }
}
