using NUnit.Framework;
using SimpleEventSourcing.Tests;

namespace SimpleEventSourcing.EntityFrameworkCore.Tests
{
    [TestFixture]
    public class PersistenceEngineEntityFrameworkTests : PersistenceEngineTests
    {
        public PersistenceEngineEntityFrameworkTests()
            : base(new EntityFrameworkCoreTestConfig())
        {

        }
    }
}
