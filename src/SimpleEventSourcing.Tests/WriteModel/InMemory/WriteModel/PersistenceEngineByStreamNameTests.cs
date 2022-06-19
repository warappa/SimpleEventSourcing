using NUnit.Framework;
using SimpleEventSourcing.WriteModel.InMemory.Tests;
using SimpleEventSourcing.WriteModel.Tests;

namespace SimpleEventSourcing.WriteModel.InMemory.WriteModel.Tests
{
    [TestFixture]
    public class PersistenceEngineByStreamNameTestsInMemory : PersistenceEngineByStreamNameTestsBase
    {
        public PersistenceEngineByStreamNameTestsInMemory()
            : base(new InMemoryTestConfig())
        {

        }
    }
}
