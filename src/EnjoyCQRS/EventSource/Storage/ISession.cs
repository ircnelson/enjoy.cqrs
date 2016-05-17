using System;
using System.Threading.Tasks;

namespace EnjoyCQRS.EventSource.Storage
{
    /// <summary>
    /// Represents an abstraction where aggregate events will be persisted.
    /// </summary>
    public interface ISession
    {
        /// <summary>
        /// Retrieves an <typeparam name="TAggregate"></typeparam> based on your unique identifier property.
        /// </summary>
        /// <typeparam name="TAggregate"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TAggregate> GetByIdAsync<TAggregate>(Guid id) where TAggregate : Aggregate, new();

        /// <summary>
        /// Add an instance of <typeparam name="TAggregate"></typeparam>.
        /// </summary>
        /// <typeparam name="TAggregate"></typeparam>
        /// <param name="aggregate"></param>
        Task AddAsync<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate;

        /// <summary>
        /// Begin the transaction.
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Confirm affected changes.
        /// </summary>
        Task CommitAsync();

        /// <summary>
        /// Save the modifications.
        /// </summary>
        Task SaveChangesAsync();

        /// <summary>
        /// Revert modifications.
        /// </summary>
        void Rollback();

    }
}