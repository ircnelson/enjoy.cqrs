using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using EnjoyCQRS.Bus;

namespace EnjoyCQRS.EventStore.Storage
{
    public class Session : ISession
    {
        private readonly IAggregateTracker _aggregateTracker;
        private readonly IEventStore _domainEventStore;
        private readonly IMessageBus _messageBus;
        private readonly List<Aggregate> _aggregates = new List<Aggregate>();

        public Session(IAggregateTracker aggregateTracker, IEventStore domainEventStore, IMessageBus messageBus)
        {
            _aggregateTracker = aggregateTracker;
            _domainEventStore = domainEventStore;
            _messageBus = messageBus;
        }

        public TAggregate GetById<TAggregate>(Guid id) where TAggregate : Aggregate, new()
        {
            var aggregate = new TAggregate();

            var events = _domainEventStore.GetAllEvents(id);

            aggregate.LoadFromHistory(events);

            RegisterForTracking(aggregate);

            return aggregate;
        }

        public void Add<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate
        {
            RegisterForTracking(aggregate);
        }

        public void Commit()
        {
            _domainEventStore.BeginTransaction();

            foreach (var aggregate in _aggregates)
            {
                var changes = aggregate.UncommitedEvents.ToList();
                
                _domainEventStore.Save(changes);

                _messageBus.Publish(changes.Select(e => (object)e));

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
                _aggregateTracker.Remove(aggregate.GetType(), aggregate.Id);
            }

            _aggregates.Clear();
        }

        private void RegisterForTracking<TAggregate>(TAggregate aggregateRoot) where TAggregate : Aggregate
        {
            _aggregates.Add(aggregateRoot);
            _aggregateTracker.Add(aggregateRoot);
        }
    }
}