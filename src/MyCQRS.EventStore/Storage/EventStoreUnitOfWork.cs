using MyCQRS.Events;
using System;
using System.Collections.Generic;

namespace MyCQRS.EventStore.Storage
{
    public class EventStoreUnitOfWork : IEventStoreUnitOfWork
    {
        private readonly IAggregateCache _aggregateCache;
        private readonly IDomainEventStore<IDomainEvent> _domainEventStore;
        private readonly List<IAggregate> _aggregates = new List<IAggregate>();

        public EventStoreUnitOfWork(IAggregateCache aggregateCache, IDomainEventStore<IDomainEvent> domainEventStore)
        {
            _aggregateCache = aggregateCache;
            _domainEventStore = domainEventStore;
        }
        
        public TAggregate GetById<TAggregate>(Guid id) where TAggregate : class, IAggregate, new()
        {
            var aggregateRoot = new TAggregate();

            var events = _domainEventStore.GetAllEvents(id);

            aggregateRoot.LoadFromHistory(events);

            return aggregateRoot;
        }

        public void Add<TAggregate>(TAggregate aggregate) where TAggregate : class, IAggregate
        {
            _aggregates.Add(aggregate);
            _aggregateCache.Add(aggregate);
        }

        public void Commit()
        {
            _domainEventStore.BeginTransaction();

            foreach (var aggregate in _aggregates)
            {
                _domainEventStore.Save(aggregate);
                // Queue events to publish further
                aggregate.ClearUncommitedEvents();
            }

            _aggregates.Clear();

            // Publish queued messages
            _domainEventStore.Commit();
        }

        public void Rollback()
        {
            // Dequeue messages!

            _domainEventStore.Rollback();

            foreach (var aggregate in _aggregates)
            {
                _aggregateCache.Remove(aggregate.GetType(), aggregate.Id);
            }

            _aggregates.Clear();
        }
    }
}