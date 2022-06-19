using SimpleEventSourcing.Messaging;
using System;

namespace SimpleEventSourcing.Tests
{
    public interface IBaseEvent : IEvent
    {
        void SetDateTime(DateTime dateTime);
    }
}