using System;

namespace SimpleEventSourcing.ReadModel
{
    public interface IDbScopeAware
    {
        IDisposable OpenScope();
    }
}
