using NUnit.Framework;
using SimpleEventSourcing.NHibernate.Tests;
using SimpleEventSourcing.Tests.Storage;
using NHibernate.Context;
using NHibernate;

namespace SimpleEventSourcing.NHibernate.WriteModel.Tests
{
    [TestFixture]
    public class StorageTests : StorageResetterTests
    {
        public StorageTests()
            : base(new NHibernateTestConfig())
        {

        }
        
        public NHibernateTestConfig NHconfig { get { return config as NHibernateTestConfig; } }

        private ISessionFactory writeSessionFactory;
        private ISessionFactory readSessionFactory;

        protected override void BeforeFixtureTransaction()
        {
            NHconfig.WriteModel.EnsureWriteDatabase();
            NHconfig.ReadModel.EnsureReadDatabase();

            base.BeforeFixtureTransaction();
        }

        [OneTimeSetUp]
        public void SetupStorageTestsFixture()
        {
            //writeSessionFactory = NHconfig.WriteModel.GetSessionFactory();
            //readSessionFactory = NHconfig.ReadModel.GetSessionFactory();
        }

        [OneTimeTearDown]
        public void TearDownStorageTestsFixture()
        {
            //readSessionFactory.Dispose();
            //readSessionFactory = null;

            //writeSessionFactory.Dispose();
            //writeSessionFactory = null;
        }

        [SetUp]
        public void SetupStorageTests()
        {
            //var writeSession = writeSessionFactory.OpenSession();

            //CurrentSessionContext.Bind(writeSession);

            //var readSession = readSessionFactory.OpenSession();

            //CurrentSessionContext.Bind(readSession);
        }

        [TearDown]
        public void TearDownStorageTests()
        {
            //CurrentSessionContext.Unbind(writeSessionFactory)?.Dispose();
            //CurrentSessionContext.Unbind(readSessionFactory)?.Dispose();
        }
    }
}
