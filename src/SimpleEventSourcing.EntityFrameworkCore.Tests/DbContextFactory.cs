using EntityFrameworkCore.DbContextScope;
using SimpleEventSourcing.EntityFrameworkCore.Tests.ReadModel;
using SimpleEventSourcing.EntityFrameworkCore.Tests.WriteModel;
using System;

namespace SimpleEventSourcing.EntityFrameworkCore.Tests
{
    public class DbContextFactory : IDbContextFactory
    {
        TDbContext IDbContextFactory.CreateDbContext<TDbContext>()
        {
            if (typeof(TDbContext) == typeof(WriteModelTestDbContext))
            {
                return (TDbContext)(object)new WriteModelTestDbContext();
            }
            else if (typeof(TDbContext) == typeof(ReadModelTestDbContext))
            {
                return (TDbContext)(object)new ReadModelTestDbContext();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
