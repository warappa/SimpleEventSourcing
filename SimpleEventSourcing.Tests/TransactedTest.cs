using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Transactions;

namespace SimpleEventSourcing.Tests
{
    public abstract class TransactedTest : IDisposable
    {
        protected bool UseTestTransaction = true;

        private TransactionScope testTransaction;

        [OneTimeSetUp]
        protected virtual async Task BeforeFixtureTransactionAsync()
        {

        }

        protected virtual async Task BeforeTestTransactionAsync()
        {

        }

        [SetUp]
        public async Task SetupTest()
        {
            await BeforeTestTransactionAsync();

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
            GC.SuppressFinalize(this);
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