using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.State
{
    public interface IState
    {
        Type[] PayloadTypes { get; }
        object UntypedApply(object eventOrMessage);
    }
    public interface ISynchronousState : IState
    {
    }
    public interface IAsyncState : IState
    {
        Task<object> UntypedApplyAsync(object eventOrMessage);
    }
}
