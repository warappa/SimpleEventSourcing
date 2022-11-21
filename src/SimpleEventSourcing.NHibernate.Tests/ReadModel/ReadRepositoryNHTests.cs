using NHibernate;
using NUnit.Framework;
using SimpleEventSourcing.Tests.ReadModel;
using System.Threading.Tasks;

namespace SimpleEventSourcing.NHibernate.Tests.ReadModel
{
    [TestFixture]
    public class ReadRepositoryNHTests : ReadRepositoryTests
    {
        private readonly ISessionFactory sessionFactory;

        public ReadRepositoryNHTests()
            : base(new NHibernateTestConfig())
        {
        }

        public NHibernateTestConfig NHconfig => config as NHibernateTestConfig;

        protected override async Task BeforeFixtureTransactionAsync()
        {
            await NHconfig.WriteModel.EnsureWriteDatabaseAsync().ConfigureAwait(false);
            await NHconfig.ReadModel.EnsureReadDatabaseAsync().ConfigureAwait(false);

            await base.BeforeFixtureTransactionAsync().ConfigureAwait(false);
        }

        [OneTimeSetUp]
        public void SetupReadRepositoryNHTestsFixture()
        {
            //sessionFactory = NHconfig.ReadModel.GetSessionFactory();
        }

        [OneTimeTearDown]
        public void TearDownReadRepositoryNHTestsFixture()
        {
            //sessionFactory.Close();
            //sessionFactory = null;
        }

        [SetUp]
        public void SetupReadRepositoryNHTests()
        {
            //var session = sessionFactory.OpenSession();

            //CurrentSessionContext.Bind(session);
        }

        [TearDown]
        public void TearDownReadRepositoryNHTests()
        {
            //CurrentSessionContext.Unbind(sessionFactory)?.Dispose();
        }
    }
}
