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
using System.Linq;
using System.Threading.Tasks;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource.Exceptions;
using EnjoyCQRS.Messages;

namespace EnjoyCQRS.EventSource.Storage
{
    public class Session : ISession
    {
        private readonly AggregateTracker _aggregateTracker;
        private readonly IEventStore _eventStore;
        private readonly IEventPublisher _eventPublisher;
        private readonly List<Aggregate> _aggregates = new List<Aggregate>();

        private bool _externalTransaction;

        public Session(IEventStore eventStore, IEventPublisher eventPublisher)
        {
            if (eventStore == null) throw new ArgumentNullException(nameof(eventStore));
            if (eventPublisher == null) throw new ArgumentNullException(nameof(eventPublisher));

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
        public async Task<TAggregate> GetByIdAsync<TAggregate>(Guid id) where TAggregate : Aggregate, new()
        {
            TAggregate aggregate = _aggregateTracker.GetById<TAggregate>(id);

            if (aggregate != null) return aggregate;
            
            var events = await _eventStore.GetAllEventsAsync(id).ConfigureAwait(false);

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
                    var changes = aggregate.UncommitedEvents.ToList();

                    await _eventStore.SaveAsync(changes).ConfigureAwait(false);

                    await _eventPublisher.PublishAsync<IDomainEvent>(changes).ConfigureAwait(false);

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
    }
}