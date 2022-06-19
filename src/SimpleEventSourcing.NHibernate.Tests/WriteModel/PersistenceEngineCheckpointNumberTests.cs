using NHibernate;
using NUnit.Framework;
using SimpleEventSourcing.NHibernate.Tests;
using SimpleEventSourcing.WriteModel.Tests;

namespace SimpleEventSourcing.NHibernate.WriteModel.Tests
{
    [TestFixture]
    public class PersistenceEngineCheckpointNumberTests : PersistenceEngineCheckpointNumberTestsBase
    {
        public NHibernateTestConfig NHconfig { get { return config as NHibernateTestConfig; } }

        private ISessionFactory sessionFactory;

        public PersistenceEngineCheckpointNumberTests()
            : base(new NHibernateTestConfig())
        {
        }

        [OneTimeSetUp]
        public void SetupStorageTestsFixture()
        {
            //sessionFactory = NHconfig.ReadModel.GetSessionFactory();
        }

        [OneTimeTearDown]
        public void TearDownStorageTestsFixture()
        {
            //sessionFactory.Close();
            //sessionFactory = null;
        }

        [SetUp]
        protected override void EarlySetup()
        {
            //var session = sessionFactory.OpenSession();

            //CurrentSessionContext.Bind(session);
        }

        [TearDown]
        public void TearDownStorageTests()
        {
            //CurrentSessionContext.Unbind(sessionFactory)?.Dispose();
        }
    }
}
