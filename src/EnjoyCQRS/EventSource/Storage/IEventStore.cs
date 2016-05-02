using System;
using System.Collections.Generic;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.EventSource.Storage
{
    /// <summary>
    /// Event Store repository abstraction.
    /// </summary>
    public interface IEventStore : ITransactional
    {
        /// <summary>
        /// Retrieve all events based on <param name="id"></param>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IEnumerable<IDomainEvent> GetAllEvents(Guid id);

        /// <summary>
        /// Save the events in Event Store.
        /// </summary>
        /// <param name="events"></param>
        void Save(IEnumerable<IDomainEvent> events);
    }
}
