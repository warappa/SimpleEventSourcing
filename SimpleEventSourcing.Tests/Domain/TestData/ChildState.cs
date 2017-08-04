using SimpleEventSourcing.State;
using System;

namespace SimpleEventSourcing.Tests
{
    public class ChildState : ChildEntityState<ChildState, ParentEntityId, ChildEntityId>
    {
		public string Name { get; private set; }

		public ChildState():base() { }

		public ChildState Apply(ChildCreated @event)
		{
			this.AggregateRootId = @event.AggregateRootId;
			this.Id = @event.Id;
			this.Name = @event.Name;

			return new ChildState { AggregateRootId = AggregateRootId, Id = Id, Name = Name };
		}

		public ChildState Apply(ChildRenamed @event)
		{
			this.Name = @event.Name;

			return new ChildState { AggregateRootId = AggregateRootId, Id = Id, Name = Name };
		}

        public override object ConvertFromStreamName(Type tkey, string streamName)
        {
            return new ChildEntityId(streamName);
        }

        public override string ConvertToStreamName(Type tkey, object id)
        {
            return ((ChildEntityId)id).Value;
        }
    }
}