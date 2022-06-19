using NUnit.Framework;
using SimpleEventSourcing.NHibernate.Tests;
using SimpleEventSourcing.WriteModel.Tests;

namespace SimpleEventSourcing.NHibernate.WriteModel.Tests
{
    [TestFixture]
    public class PersistenceEngineByStreamNameTestsNHibernate : PersistenceEngineByStreamNameTestsBase
    {
        public PersistenceEngineByStreamNameTestsNHibernate()
            : base(new NHibernateTestConfig())
        {
        }
    }
}
