using System;

namespace SimpleEventSourcing.State
{
    public interface IState
    {
        Type[] PayloadTypes { get; }
        object UntypedApply(object eventOrMessage);
    }
}
