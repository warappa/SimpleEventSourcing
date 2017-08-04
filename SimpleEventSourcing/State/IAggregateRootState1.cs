using System;
using System.Collections.Generic;

namespace SimpleEventSourcing.State
{
    public interface IAggregateRootState<TKey> : IAggregateRootState
    {
        new TKey Id { get; }
    }
}
