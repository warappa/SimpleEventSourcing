﻿using NHibernate;
using NUnit.Framework;
using SimpleEventSourcing.Tests.Storage;
using System.Threading.Tasks;

namespace SimpleEventSourcing.NHibernate.Tests.Storage
{
    [TestFixture]
    public class StorageTests : StorageResetterTests
    {
        public StorageTests()
            : base(new NHibernateTestConfig())
        {

        }

        public NHibernateTestConfig NHconfig => config as NHibernateTestConfig;

        private readonly ISessionFactory writeSessionFactory;
        private readonly ISessionFactory readSessionFactory;

        protected override async Task BeforeFixtureTransactionAsync()
        {
            await NHconfig.WriteModel.EnsureWriteDatabaseAsync().ConfigureAwait(false);
            await NHconfig.ReadModel.EnsureReadDatabaseAsync().ConfigureAwait(false);

            await base.BeforeFixtureTransactionAsync().ConfigureAwait(false);
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
