using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.Utils;
using System;

namespace SimpleEventSourcing.Tests
{
    public abstract class BaseEvent : IBaseEvent, IEventSourcedEntityEvent
    {
        protected BaseEvent(string id, DateTime dateTime)
        {
            Id = id;
            DateTime = dateTime;
        }

        public string Id { get; protected set; }
        public DateTime DateTime { get; protected set; }

		object IEventSourcedEntityEvent.Id => Id;

		void IBaseEvent.SetDateTime(DateTime dateTime)
		{
			DateTime = dateTime;
		}

		public override bool Equals(object obj)
		{
			return this.PropertyEqualtity(obj);
		}
		public override int GetHashCode()
		{
			return this.PropertyHashCode();
		}
	}
}