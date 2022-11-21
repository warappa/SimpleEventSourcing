using NHibernate;
using NUnit.Framework;
using SimpleEventSourcing.Tests.WriteModel;
using System.Threading.Tasks;

namespace SimpleEventSourcing.NHibernate.Tests.WriteModel
{
    [TestFixture]
    public class PersistenceEngineBasicTests : PersistenceEngineBasicTestsBase
    {
        public NHibernateTestConfig NHconfig => config as NHibernateTestConfig;

        private readonly ISessionFactory readSessionFactory;
        private readonly ISessionFactory writeSessionFactory;

        public PersistenceEngineBasicTests()
            : base(new NHibernateTestConfig(), false)
        {
            UseTestTransaction = true;
        }

        [Test]
        public new async Task Can_initializeAsync()
        {
            await InitializeAsync().ConfigureAwait(false);
        }

        [Test]
        public new async Task Can_save_streamEntriesAsync()
        {
            await InitializeAsync().ConfigureAwait(false);

            await SaveStreamEntryAsync().ConfigureAwait(false);
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
