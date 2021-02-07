using System;
using System.Transactions;

namespace System.Transactions
{
    public class AsyncTransactionScope : IDisposable
    {
        private TransactionScope scope;

        public AsyncTransactionScope()
        {
            scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        }
        public AsyncTransactionScope(Transaction transactionToUse)
        {
            scope = new TransactionScope(transactionToUse, TransactionScopeAsyncFlowOption.Enabled);
        }
        
        public AsyncTransactionScope(TransactionScopeOption scopeOption)
        {
            scope = new TransactionScope(scopeOption, TransactionScopeAsyncFlowOption.Enabled);
        }

        public AsyncTransactionScope(Transaction transactionToUse, TimeSpan scopeTimeout)
        {
            scope = new TransactionScope(transactionToUse, scopeTimeout, TransactionScopeAsyncFlowOption.Enabled);
        }

        public AsyncTransactionScope(TransactionScopeOption scopeOption, TimeSpan scopeTimeout)
        {
            scope = new TransactionScope(scopeOption, scopeTimeout, TransactionScopeAsyncFlowOption.Enabled);
        }

        public AsyncTransactionScope(TransactionScopeOption scopeOption, TransactionOptions transactionOptions)
        {
            scope = new TransactionScope(scopeOption, transactionOptions, TransactionScopeAsyncFlowOption.Enabled);
        }

        //
        // Summary:
        //     Indicates that all operations within the scope are completed successfully.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     This method has already been called once.
        public void Complete()
        {
            scope.Complete();
        }

        //
        // Summary:
        //     Ends the transaction scope.
        public void Dispose()
        {
            scope.Dispose();
        }
    }
}
