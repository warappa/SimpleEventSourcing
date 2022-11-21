using NUnit.Framework;

namespace SimpleEventSourcing.Tests.WriteModel.InMemory.WriteModel
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
