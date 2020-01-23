using System;
using System.Threading.Tasks;

namespace GenderPayGap.Core.Interfaces
{
    public interface IDataTransaction
    {

        /// <summary>
        ///     Creates a transaction for aggregating data update operations
        /// </summary>
        /// <param name="transactionFunc"></param>
        Task BeginTransactionAsync(Func<Task> transactionFunc);

        void CommitTransaction();

        void RollbackTransaction();

    }

}
