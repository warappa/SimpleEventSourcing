using EntityFrameworkCore.DbContextScope;

namespace SimpleEventSourcing.EntityFrameworkCore.Tests
{
    public class ScopeHelper
    {
        private readonly IDbContextScopeFactory dbContextScopeFactory;
        private IDbContextScope fixtureScope;
        private IDbContextScope testScope;

        public ScopeHelper(IDbContextScopeFactory dbContextScopeFactory)
        {
            this.dbContextScopeFactory = dbContextScopeFactory;
        }

        public void CreateFixtureScope()
        {
            fixtureScope = dbContextScopeFactory.Create(DbContextScopeOption.ForceCreateNew);
        }

        public void DisposeFixtureScope()
        {
            fixtureScope?.Dispose();
            fixtureScope = null;
        }

        public void CreateTestScope()
        {
            testScope = dbContextScopeFactory.Create(DbContextScopeOption.JoinExisting);
        }

        public void DisposeTestScope()
        {
            testScope?.Dispose();
            testScope = null;
        }
    }
}
