using NUnit.Framework;
using SimpleEventSourcing.EntityFrameworkCore.Tests;
using SimpleEventSourcing.WriteModel.Tests;

namespace SimpleEventSourcing.EntityFrameworkCore.WriteModel.Tests
{
    [TestFixture]
    public class PersistenceEngineByStreamNameTestsEntityFramework : PersistenceEngineByStreamNameTestsBase
    {
        public PersistenceEngineByStreamNameTestsEntityFramework()
            : base(new EntityFrameworkCoreTestConfig())
        {
        }
    }
}
