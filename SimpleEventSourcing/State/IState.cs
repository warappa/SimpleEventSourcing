using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.State
{
    public interface IState
    {
        Type[] PayloadTypes { get; }
        Task<object> UntypedApplyAsync(object eventOrMessage);
    }
}
