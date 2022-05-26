using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.State
{
    public interface IProjector
    {
        Type[] PayloadTypes { get; }
        object UntypedApply(object eventOrMessage);
    }
    public interface ISynchronousProjector : IProjector
    {
    }
    public interface IAsyncProjector : IProjector
    {
        Task<object> UntypedApplyAsync(object eventOrMessage);
    }
}
