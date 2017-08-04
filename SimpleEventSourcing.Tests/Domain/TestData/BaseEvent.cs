using SimpleEventSourcing.Messaging;
using System;
using SimpleEventSourcing.Utils;

namespace SimpleEventSourcing.Tests
{
    public abstract class BaseEvent : IBaseEvent, IEventSourcedEntityEvent
    {
        public BaseEvent(string id, DateTime dateTime)
        {
            this.Id = id;
            this.DateTime = dateTime;
        }

        public string Id { get; protected set; }
        public DateTime DateTime { get; protected set; }

		object IEventSourcedEntityEvent.Id => this.Id;

		void IBaseEvent.SetDateTime(DateTime dateTime)
		{
			this.DateTime = dateTime;
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