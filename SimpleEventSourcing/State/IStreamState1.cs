using System;

namespace SimpleEventSourcing.State
{
    public interface IStreamState<TState> : IStreamState, IEventSourcedState<TState>
        where TState : IStreamState, IStreamState<TState>, new()
    {
        object ConvertFromStreamName(Type tkey, string streamName);
        string ConvertToStreamName(Type tkey, object id);
    }
}
