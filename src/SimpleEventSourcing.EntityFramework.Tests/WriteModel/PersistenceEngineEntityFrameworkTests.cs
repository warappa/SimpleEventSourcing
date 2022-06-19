using NUnit.Framework;
using SimpleEventSourcing.Tests;

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
