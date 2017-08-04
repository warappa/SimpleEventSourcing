using NHibernate;
using NHibernate.Context;
using NUnit.Framework;
using SimpleEventSourcing.NHibernate.Tests;
using SimpleEventSourcing.ReadModel.Tests;

namespace SimpleEventSourcing.NHibernate.ReadModel.Tests
{
    [TestFixture]
    public class ReadRepositoryNHTests : ReadRepositoryTests
    {
        private ISessionFactory sessionFactory;

        public ReadRepositoryNHTests()
            : base(new NHibernateTestConfig())
        {
        }

        public NHibernateTestConfig NHconfig { get { return config as NHibernateTestConfig; } }

        protected override void BeforeFixtureTransaction()
        {
            NHconfig.WriteModel.EnsureWriteDatabase();
            NHconfig.ReadModel.EnsureReadDatabase();

            base.BeforeFixtureTransaction();
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
