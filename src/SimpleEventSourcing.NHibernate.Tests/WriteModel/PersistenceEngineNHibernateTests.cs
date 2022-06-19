using NUnit.Framework;
using SimpleEventSourcing.Tests;

namespace SimpleEventSourcing.NHibernate.Tests
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
