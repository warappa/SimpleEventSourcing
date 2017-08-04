using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Transactions;

namespace SimpleEventSourcing.Tests
{
    public abstract class TransactedTest : IDisposable
    {
        protected bool UseTestTransaction = true;

        private TransactionScope testTransaction;

        [OneTimeSetUp]
        protected virtual void BeforeFixtureTransaction()
        {

        }

        protected virtual void BeforeTestTransaction()
        {

        }

        [SetUp]
        public void SetupTest()
        {
            BeforeTestTransaction();

            if (UseTestTransaction)
            {
                return;
                Debug.WriteLine("Start test transactionscope");
                testTransaction = new TransactionScope(
                    TransactionScopeOption.RequiresNew,
                    new TransactionOptions
                    {
                        Timeout = TimeSpan.FromSeconds(20)
                    },
                    TransactionScopeAsyncFlowOption.Enabled);
            }
        }

        [TearDown]
        public void TeardownTest()
        {
            testTransaction?.Dispose();
            testTransaction = null;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                testTransaction?.Dispose();
            }
        }
    }
}