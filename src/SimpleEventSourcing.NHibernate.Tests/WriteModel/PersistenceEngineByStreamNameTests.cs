using NUnit.Framework;
using SimpleEventSourcing.Tests.WriteModel;

namespace SimpleEventSourcing.NHibernate.Tests.WriteModel
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
