﻿using NHibernate;
using NUnit.Framework;
using SimpleEventSourcing.NHibernate.Tests;
using SimpleEventSourcing.ReadModel.Tests;
using System.Threading.Tasks;

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

        protected override async Task BeforeFixtureTransactionAsync()
        {
            await NHconfig.WriteModel.EnsureWriteDatabaseAsync();
            await NHconfig.ReadModel.EnsureReadDatabaseAsync();

            await base.BeforeFixtureTransactionAsync();
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
