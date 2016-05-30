// The MIT License (MIT)
// 
// Copyright (c) 2016 Nelson Corrêa V. Júnior
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnjoyCQRS.Collections;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource.Exceptions;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.MessageBus;

namespace EnjoyCQRS.EventSource.Storage
{
    public class Session : ISession
    {
        private readonly AggregateTracker _aggregateTracker = new AggregateTracker();
        private readonly IEventStore _eventStore;
        private readonly IEventPublisher _eventPublisher;
        private readonly List<Aggregate> _aggregates = new List<Aggregate>();
        private readonly ISnapshotStrategy _snapshotStrategy;

        private bool _externalTransaction;

        public Session(IEventStore eventStore, IEventPublisher eventPublisher, ISnapshotStrategy snapshotStrategy = null)
        {
            if (eventStore == null) throw new ArgumentNullException(nameof(eventStore));
            if (eventPublisher == null) throw new ArgumentNullException(nameof(eventPublisher));
            
            if (snapshotStrategy == null)
            {
                snapshotStrategy = new IntervalSnapshotStrategy();
            }

            _snapshotStrategy = snapshotStrategy;
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
        public async Task<TAggregate> GetByIdAsync<TAggregate>(Guid id) where TAggregate : Aggregate, new()
        {
            var aggregate = _aggregateTracker.GetById<TAggregate>(id);

            if (aggregate != null) return aggregate;

            aggregate = new TAggregate();

            IEnumerable<IDomainEvent> events;

            if (_snapshotStrategy.CheckSnapshotSupport(aggregate.GetType()))
            {
                var snapshotAggregate = aggregate as ISnapshotAggregate;
                if (snapshotAggregate != null)
                {
                    var snapshot = await _eventStore.GetSnapshotByIdAsync(id).ConfigureAwait(false);

                    snapshotAggregate.Restore(snapshot);

                    events = await _eventStore.GetEventsForwardAsync(id, snapshot.Version).ConfigureAwait(false);

                    LoadAggregate(aggregate, events);
                }
            }
            else
            {
                events = await _eventStore.GetAllEventsAsync(id).ConfigureAwait(false);

                LoadAggregate(aggregate, events);
            }

            if (aggregate.Id.Equals(Guid.Empty)) throw new AggregateNotFoundException(typeof(TAggregate).Name, id);

            RegisterForTracking(aggregate);

            return aggregate;
        }
        
        /// <summary>
        /// Add the aggregate to tracking.
        /// </summary>
        /// <typeparam name="TAggregate"></typeparam>
        /// <param name="aggregate"></param>
        public Task AddAsync<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate
        {
            CheckConcurrency(aggregate);

            RegisterForTracking(aggregate);

            return Task.CompletedTask;
        }

        public void BeginTransaction()
        {
            if (_externalTransaction) throw new InvalidOperationException("The transaction already was open.");

            _externalTransaction = true;
            _eventStore.BeginTransaction();
        }

        public async Task CommitAsync()
        {
            await _eventStore.CommitAsync().ConfigureAwait(false);
            _externalTransaction = false;
        }

        /// <summary>
        /// Call <see cref="IEventStore.SaveAsync"/> in <see cref="IEventStore"/> passing aggregates.
        /// </summary>
        public virtual async Task SaveChangesAsync()
        {
            if (!_externalTransaction)
            {
                _eventStore.BeginTransaction();
            }

            // If transaction called externally, the client should care with transaction.
            try
            {
                foreach (var aggregate in _aggregates)
                {
                    var aggregateMetadata = new AggregateMetadata(aggregate.Id, aggregate.GetType());

                    var events = new UncommitedDomainEventCollection(aggregateMetadata, aggregate.UncommitedEvents);

                    if (_snapshotStrategy.ShouldMakeSnapshot(aggregate))
                    {
                        await TakeSnapshot(aggregate).ConfigureAwait(false);
                    }
                    
                    await _eventStore.SaveAsync(events).ConfigureAwait(false);

                    await _eventPublisher.PublishAsync<IDomainEvent>(events).ConfigureAwait(false);

                    aggregate.UpdateVersion(aggregate.EventVersion);

                    aggregate.ClearUncommitedEvents();
                }

                _aggregates.Clear();

                await _eventPublisher.CommitAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                if (!_externalTransaction)
                {
                    Rollback();
                }

                throw;
            }
            
            if (!_externalTransaction)
            {
                await _eventStore.CommitAsync().ConfigureAwait(false);
            }
        }

        private async Task TakeSnapshot(Aggregate aggregate)
        {
            var snapshot = ((ISnapshotAggregate)aggregate).CreateSnapshot();

            await _eventStore.SaveSnapshotAsync(snapshot).ConfigureAwait(false);
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

        private void CheckConcurrency<TAggregate>(TAggregate aggregateRoot) where TAggregate : Aggregate
        {
            var trackedAggregate = _aggregateTracker.GetById<TAggregate>(aggregateRoot.Id);

            if (trackedAggregate == null) return;

            if (trackedAggregate.Version != aggregateRoot.Version)
            {
                throw new ExpectedVersionException<TAggregate>(aggregateRoot, trackedAggregate.Version);
            }
        }

        private void LoadAggregate<TAggregate>(TAggregate aggregate, IEnumerable<IDomainEvent> events) where TAggregate : Aggregate
        {
            aggregate.LoadFromHistory(new CommitedDomainEventCollection(events));
        }
    }
}