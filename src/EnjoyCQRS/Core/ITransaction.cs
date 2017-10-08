using System;
using System.Threading.Tasks;

namespace EnjoyCQRS.Core
{
    public interface ITransaction : IDisposable
    {
        /// <summary>
        /// Start the transaction.
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Confirm modifications.
        /// </summary>
        Task CommitAsync();

        /// <summary>
        /// Revert modifications.
        /// </summary>
        void Rollback();
    }
}
