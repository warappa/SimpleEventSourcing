using NHibernate;
using NHibernate.Context;
using System;

namespace SimpleEventSourcing.NHibernate.ReadModel
{
    public class CurrentSessionContextDisposer : IDisposable
    {
        private readonly ISessionFactory sessionFactory;

        public CurrentSessionContextDisposer(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                var session = CurrentSessionContext.Unbind(sessionFactory);
                if (session == null)
                {
                    return;
                }

                session.Flush();
                if (session.GetCurrentTransaction()?.IsActive == true)
                {
                    session.GetCurrentTransaction().Commit();
                }

                session.Dispose();
            }
        }
    }

    public class FlushDisposer : IDisposable
    {
        private readonly ISession session;

        public FlushDisposer(ISession session)
        {
            this.session = session;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                session.Flush();
            }
        }
    }
}
