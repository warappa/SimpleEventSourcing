using System;

namespace SimpleEventSourcing.ReadModel
{
    public interface IProjector
    {
        IDisposable Start();
    }
}
