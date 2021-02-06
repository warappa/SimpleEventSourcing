using NHibernate;
using NUnit.Framework;
using SimpleEventSourcing.NHibernate.Tests;
using SimpleEventSourcing.WriteModel.Tests;
using System.Threading.Tasks;

namespace SimpleEventSourcing.NHibernate.WriteModel.Tests
{
    [TestFixture]
    public class PersistenceEngineBasicTests : PersistenceEngineTestsBase
    {
        public NHibernateTestConfig NHconfig { get { return config as NHibernateTestConfig; } }

        private readonly ISessionFactory readSessionFactory;
        private readonly ISessionFactory writeSessionFactory;

        public PersistenceEngineBasicTests()
            : base(new NHibernateTestConfig(), false)
        {
            UseTestTransaction = true;
        }

        [Test]
        public async Task Can_initializeAsync()
        {
            await InitializeAsync();
        }

        [Test]
        public async Task Can_save_streamEntriesAsync()
        {
            await InitializeAsync();

            await SaveStreamEntryAsync();
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
