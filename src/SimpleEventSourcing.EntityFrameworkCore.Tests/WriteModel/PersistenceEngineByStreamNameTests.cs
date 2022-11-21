using NUnit.Framework;
using SimpleEventSourcing.Tests.WriteModel;

namespace SimpleEventSourcing.EntityFrameworkCore.Tests.WriteModel
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
