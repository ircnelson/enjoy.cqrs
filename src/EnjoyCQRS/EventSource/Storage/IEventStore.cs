using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.EventSource.Storage
{
    /// <summary>
    /// Event Store repository abstraction.
    /// </summary>
    public interface IEventStore : IDisposable
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

        /// <summary>
        /// Retrieve all events based on <param name="id"></param>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<IEnumerable<IDomainEvent>> GetAllEventsAsync(Guid id);

        /// <summary>
        /// Save the events in Event Store.
        /// </summary>
        /// <param name="events"></param>
        Task SaveAsync(IEnumerable<IDomainEvent> events);
    }
}
