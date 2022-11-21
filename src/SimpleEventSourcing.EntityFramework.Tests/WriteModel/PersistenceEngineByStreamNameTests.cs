using NUnit.Framework;
using SimpleEventSourcing.Tests.WriteModel;

namespace SimpleEventSourcing.EntityFramework.Tests.WriteModel
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
