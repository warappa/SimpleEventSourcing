using NHibernate;
using NHibernate.Context;
using NUnit.Framework;
using SimpleEventSourcing.NHibernate.Tests;
using SimpleEventSourcing.ReadModel.Tests;

namespace SimpleEventSourcing.NHibernate.ReadModel.Tests
{
    [TestFixture]
    public class CatchUpProjectorWithAutoReadModelResetNHibernateTests : CatchUpProjectorWithAutoReadModelResetTests
    {
        public NHibernateTestConfig NHconfig { get { return config as NHibernateTestConfig; } }

        private ISessionFactory sessionFactory;

        protected override void BeforeFixtureTransaction()
        {
            NHconfig.WriteModel.EnsureWriteDatabase();
            NHconfig.ReadModel.EnsureReadDatabase();

            base.BeforeFixtureTransaction();
        }

        public CatchUpProjectorWithAutoReadModelResetNHibernateTests()
            : base(new NHibernateTestConfig())
        {
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
