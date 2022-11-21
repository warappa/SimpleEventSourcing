using NUnit.Framework;
using SimpleEventSourcing.Tests.WriteModel;

namespace SimpleEventSourcing.NHibernate.Tests.WriteModel
{
    [TestFixture]
    public class PersistenceEngineNHibernateTests : PersistenceEngineTests
    {
        public PersistenceEngineNHibernateTests()
            : base(new NHibernateTestConfig())
        {

        }
    }
}
