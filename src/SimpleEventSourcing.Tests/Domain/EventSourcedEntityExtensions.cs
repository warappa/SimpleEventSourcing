﻿using SimpleEventSourcing.Domain;
using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.State;
using SimpleEventSourcing.Tests.Domain.TestData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleEventSourcing.Tests.Domain
{
    public static class EventSourcedEntityExtensions
    {
        public static IAggregateRoot AsIAggregateRoot(this IAggregateRoot entity)
        {
            return entity;
        }

        public static IProcessManager AsIProcessManager(this IProcessManager entity)
        {
            return entity;
        }

        public static IEventSourcedEntity AsIEventSourcedEntity(this IEventSourcedEntity entity)
        {
            return entity;
        }

        public static IAggregateRootStateInternal AsIAggregateRootStateInternal(this IAggregateRootStateInternal entity)
        {
            return entity;
        }

        public static IEnumerable<IEvent> UncommittedEvents(this IEventSourcedEntity entity)
        {
            return entity.AsIEventSourcedEntity().UncommittedEvents;
        }

        public static T FixDateTime<T>(this T entity, DateTime value)
            where T : class, IEventSourcedEntity
        {
            entity.UncommittedEvents.OfType<IBaseEvent>().ToList().ForEach(x => x.SetDateTime(value));

            return entity;
        }
    }
}
