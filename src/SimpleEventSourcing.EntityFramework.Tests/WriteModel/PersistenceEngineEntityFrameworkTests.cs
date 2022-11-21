using NUnit.Framework;
using SimpleEventSourcing.Tests.WriteModel;

namespace SimpleEventSourcing.EntityFramework.Tests.WriteModel
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
