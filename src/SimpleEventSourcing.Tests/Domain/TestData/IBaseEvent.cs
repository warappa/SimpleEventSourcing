using SimpleEventSourcing.Messaging;
using System;

namespace SimpleEventSourcing.Tests.Domain.TestData
{
    public interface IBaseEvent : IEvent
    {
        void SetDateTime(DateTime dateTime);
    }
}
