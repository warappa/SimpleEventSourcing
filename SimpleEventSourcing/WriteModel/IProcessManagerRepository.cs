using SimpleEventSourcing.Domain;
using System;

namespace SimpleEventSourcing.WriteModel
{
    public interface IProcessManagerRepository
    {
        IProcessManager Get(Type processManagerType, string processId);
        void Save(IProcessManager processManager);
    }
}
