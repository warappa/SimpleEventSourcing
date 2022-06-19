using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.WriteModel
{
    public interface IObserveRawStreamEntries : IObservable<IRawStreamEntry>, IDisposable
    {
        Task StartAsync();
        Task<bool> PollNowAsync();
    }
}
