using System;
using System.Collections.Generic;
using System.Linq;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource.Exceptions;

namespace EnjoyCQRS.EventSource.Storage
{
    public class Session : ISession
    {
        private readonly AggregateTracker _aggregateTracker;
        private readonly IEventStore _eventStore;
        private readonly IEventPublisher _eventPublisher;
        private readonly List<Aggregate> _aggregates = new List<Aggregate>();

        public bool InTransaction { get; private set; }

        public Session(IEventStore eventStore, IEventPublisher eventPublisher)
        {
            _aggregateTracker = new AggregateTracker();
            _eventStore = eventStore;
            _eventPublisher = eventPublisher;
        }
        
        /// <summary>
        /// Retrieves an aggregate, load your historical events and add to tracking.
        /// </summary>
        /// <typeparam name="TAggregate"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="AggregateNotFoundException"></exception>
        public TAggregate GetById<TAggregate>(Guid id) where TAggregate : Aggregate, new()
        {
            TAggregate aggregate = _aggregateTracker.GetById<TAggregate>(id);

            if (aggregate != null) return aggregate;
            
            var events = _eventStore.GetAllEvents(id);

            if (events == null) throw new AggregateNotFoundException(typeof(TAggregate).Name, id);

            aggregate = new TAggregate();

            aggregate.LoadFromHistory(events);

            RegisterForTracking(aggregate);

            return aggregate;
        }

        /// <summary>
        /// Add the aggregate to tracking.
        /// </summary>
        /// <typeparam name="TAggregate"></typeparam>
        /// <param name="aggregate"></param>
        public void Add<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate
        {
            RegisterForTracking(aggregate);
        }

        public void BeginTransaction()
        {
            if (InTransaction) throw new InvalidOperationException("The transaction already was open.");

            InTransaction = true;
            _eventStore.BeginTransaction();
        }

        public void Commit()
        {
            _eventStore.Commit();
            InTransaction = false;
        }

        /// <summary>
        /// Call <see cref="IEventStore.Save"/> in <see cref="IEventStore"/> passing aggregates.
        /// </summary>
        public virtual void SaveChanges()
        {
            if (!InTransaction)
            {
                _eventStore.BeginTransaction();
            }

            foreach (var aggregate in _aggregates)
            {
                var changes = aggregate.UncommitedEvents.ToList();
                
                _eventStore.Save(changes);

                _eventPublisher.Publish<IDomainEvent>(changes);

                aggregate.ClearUncommitedEvents();
            }

            _aggregates.Clear();

            _eventPublisher.Commit();

            if (!InTransaction)
            {
                _eventStore.Commit();
            }
        }

        /// <summary>
        /// Rollback <see cref="IEventPublisher"/>, <see cref="IEventStore"/> and remove aggregate tracking.
        /// </summary>
        public void Rollback()
        {
            _eventPublisher.Rollback();

            _eventStore.Rollback();

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