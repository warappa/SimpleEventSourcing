using NHibernate;
using NHibernate.Context;
using NUnit.Framework;
using SimpleEventSourcing.NHibernate.Tests;
using SimpleEventSourcing.WriteModel.Tests;

namespace SimpleEventSourcing.NHibernate.WriteModel.Tests
{
    [TestFixture]
    public class PersistenceEngineBasicTests : PersistenceEngineTestsBase
    {
        public NHibernateTestConfig NHconfig { get { return config as NHibernateTestConfig; } }

        private ISessionFactory readSessionFactory;
        private ISessionFactory writeSessionFactory;

        public PersistenceEngineBasicTests()
            : base(new NHibernateTestConfig(), false)
        {
            UseTestTransaction = true;
        }

        [Test]
        public void Can_initialize()
        {
            Initialize();
        }

        [Test]
        public void Can_save_streamEntries()
        {
            Initialize();
            
            SaveStreamEntry();
        }


        [OneTimeSetUp]
        public void SetupStorageTestsFixture()
        {
            //readSessionFactory = NHconfig.ReadModel.GetSessionFactory();
            //writeSessionFactory = NHconfig.WriteModel.GetSessionFactory();
        }

        [OneTimeTearDown]
        public void TearDownStorageTestsFixture()
        {
            //writeSessionFactory.Close();
            //writeSessionFactory = null;

            //readSessionFactory.Close();
            //readSessionFactory = null;
        }

        protected override void EarlySetup()
        {
            //var readSession = readSessionFactory.OpenSession();
            //CurrentSessionContext.Bind(readSession);

            //var writeSession = writeSessionFactory.OpenSession();
            //CurrentSessionContext.Bind(writeSession);
        }

        [TearDown]
        public void TearDownStorageTests()
        {
            //CurrentSessionContext.Unbind(readSessionFactory)?.Dispose();
            //CurrentSessionContext.Unbind(writeSessionFactory)?.Dispose();
        }
    }
}
