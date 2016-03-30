using System;
using System.Collections.Generic;
using MyCQRS.Events;

namespace MyCQRS.EventStore.Storage
{
    /// <summary>
    /// Event Store repository abstraction.
    /// </summary>
    /// <typeparam name="TDomainEvent"></typeparam>
    public interface IDomainEventStore<TDomainEvent> : ITransactional
        where TDomainEvent : IDomainEvent
    {

        /// <summary>
        /// Retrieve all events based on <param name="aggregateId"></param>.
        /// </summary>
        /// <param name="aggregateId"></param>
        /// <returns></returns>
        IEnumerable<TDomainEvent> GetAllEvents(Guid aggregateId);

        /// <summary>
        /// Save the aggregate events in Event Store.
        /// </summary>
        /// <param name="aggregate"></param>
        void Save(IAggregate aggregate);
    }
}
