using NUnit.Framework;
using SimpleEventSourcing.EntityFramework.Tests;
using SimpleEventSourcing.WriteModel.Tests;

namespace SimpleEventSourcing.EntityFramework.WriteModel.Tests
{
    [TestFixture]
    public class PersistenceEngineByStreamNameTestsEntityFramework : PersistenceEngineByStreamNameTestsBase
    {
        public PersistenceEngineByStreamNameTestsEntityFramework()
            : base(new EntityFrameworkTestConfig())
        {
        }
    }
}
