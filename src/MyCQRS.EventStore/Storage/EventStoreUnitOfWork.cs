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

        //TODO: Need inject Bus service
        public EventStoreUnitOfWork(IAggregateCache aggregateCache, IDomainEventStore<IDomainEvent> domainEventStore)
        {
            _aggregateCache = aggregateCache;
            _domainEventStore = domainEventStore;
        }
        
        public TAggregate GetById<TAggregate>(Guid id) where TAggregate : class, IAggregate, new()
        {
            var aggregate = new TAggregate();

            var events = _domainEventStore.GetAllEvents(id);

            aggregate.LoadFromHistory(events);

            RegisterForTracking(aggregate);

            return aggregate;
        }

        public void Add<TAggregate>(TAggregate aggregate) where TAggregate : class, IAggregate, new()
        {
            RegisterForTracking(aggregate);
        }

        public void Commit()
        {
            _domainEventStore.BeginTransaction();

            foreach (var aggregate in _aggregates)
            {
                _domainEventStore.Save(aggregate);
                //TODO: Queue events to publish further
                aggregate.ClearUncommitedEvents();
            }

            _aggregates.Clear();

            //TODO: Publish queued messages
            _domainEventStore.Commit();
        }

        public void Rollback()
        {
            //TODO: Dequeue messages!

            _domainEventStore.Rollback();

            foreach (var aggregate in _aggregates)
            {
                _aggregateCache.Remove(aggregate.GetType(), aggregate.Id);
            }

            _aggregates.Clear();
        }

        private void RegisterForTracking<TAggregate>(TAggregate aggregateRoot) where TAggregate : class, IAggregate, new()
        {
            _aggregates.Add(aggregateRoot);
            _aggregateCache.Add(aggregateRoot);
        }
    }
}