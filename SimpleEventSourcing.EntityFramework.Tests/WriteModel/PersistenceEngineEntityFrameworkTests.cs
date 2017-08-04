using EntityFramework.DbContextScope;
using NUnit.Framework;
using SimpleEventSourcing.Tests;
using SimpleEventSourcing.WriteModel;
using SimpleEventSourcing.EntityFramework.WriteModel;

namespace SimpleEventSourcing.EntityFramework.Tests
{
    [TestFixture]
    public class PersistenceEngineEntityFrameworkTests : PersistenceEngineTests
    {
        public PersistenceEngineEntityFrameworkTests()
            : base(new EntityFrameworkTestConfig())
        {

        }
    }
}
