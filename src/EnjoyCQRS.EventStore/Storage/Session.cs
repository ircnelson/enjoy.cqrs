using System;
using System.Collections.Generic;
using System.Linq;
using EnjoyCQRS.Bus;

namespace EnjoyCQRS.EventStore.Storage
{
    public class Session : ISession
    {
        private readonly IAggregateTracker _aggregateCache;
        private readonly IEventStore _domainEventStore;
        private readonly IMessageBus _messageBus;
        private readonly List<IAggregate> _aggregates = new List<IAggregate>();
        
        public Session(IAggregateTracker aggregateCache, IEventStore domainEventStore, IMessageBus messageBus)
        {
            _aggregateCache = aggregateCache;
            _domainEventStore = domainEventStore;
            _messageBus = messageBus;
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
                _messageBus.Publish(aggregate.UncommitedEvents.Select(e => (object)e));
                aggregate.ClearUncommitedEvents();
            }

            _aggregates.Clear();

            _messageBus.Commit();
            _domainEventStore.Commit();
        }

        public void Rollback()
        {
            _messageBus.Rollback();

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