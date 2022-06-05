using EntityFrameworkCore.DbContextScope;
using Microsoft.EntityFrameworkCore;
using System;

namespace SimpleEventSourcing.EntityFrameworkCore.ReadModel
{
    public partial class ReadRepository<TDbContext> where TDbContext : DbContext
    {
        private class ScopeCommitter : IDisposable
        {
            private readonly IDbContextScope scope;

            public ScopeCommitter(IDbContextScope scope)
            {
                this.scope = scope;
            }

            public void Dispose()
            {
                Dispose(true);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    scope.DbContexts.Get<TDbContext>().ChangeTracker.DetectChanges();

                    scope.SaveChanges();
                    scope.Dispose();
                }
            }
        }
    }
}
