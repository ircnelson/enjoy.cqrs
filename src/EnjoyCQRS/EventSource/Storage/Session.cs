using System;
using System.Collections.Generic;
using System.Linq;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.EventSource.Storage
{
    public class Session : ISession
    {
        private readonly IAggregateTracker _aggregateTracker;
        private readonly IEventStore _domainEventStore;
        private readonly IEventPublisher _eventPublisher;
        private readonly List<Aggregate> _aggregates = new List<Aggregate>();

        public Session(IAggregateTracker aggregateTracker, IEventStore domainEventStore, IEventPublisher eventPublisher)
        {
            _aggregateTracker = aggregateTracker;
            _domainEventStore = domainEventStore;
            _eventPublisher = eventPublisher;
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

                _eventPublisher.Publish(changes);

                aggregate.ClearUncommitedEvents();
            }

            _aggregates.Clear();

            _eventPublisher.Commit();
            _domainEventStore.Commit();
        }

        public void Rollback()
        {
            _eventPublisher.Rollback();

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