using System;

namespace SimpleEventSourcing.ReadModel
{
    public sealed class NullDisposable : IDisposable
    {
        public void Dispose()
        {
            // do nothing
        }
    }
}
