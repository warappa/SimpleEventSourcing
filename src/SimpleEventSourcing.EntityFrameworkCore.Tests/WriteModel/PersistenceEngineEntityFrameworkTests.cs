using NUnit.Framework;
using SimpleEventSourcing.Tests.WriteModel;

namespace SimpleEventSourcing.EntityFrameworkCore.Tests.WriteModel
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
