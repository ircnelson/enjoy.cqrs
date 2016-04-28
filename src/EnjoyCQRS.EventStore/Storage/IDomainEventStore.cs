using System;
using System.Collections.Generic;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.EventStore.Storage
{
    /// <summary>
    /// Event Store repository abstraction.
    /// </summary>
    public interface IDomainEventStore : ITransactional
    {
        /// <summary>
        /// Retrieve all events based on <param name="id"></param>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IEnumerable<IDomainEvent> GetAllEvents(Guid id);

        /// <summary>
        /// Save the aggregate events in Event Store.
        /// </summary>
        /// <param name="aggregate"></param>
        void Save(IAggregate aggregate);
    }
}
