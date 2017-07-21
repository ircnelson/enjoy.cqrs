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
using EnjoyCQRS.Collections;
using EnjoyCQRS.Core;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource.Exceptions;
using EnjoyCQRS.EventSource.Projections;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.Extensions;
using EnjoyCQRS.Logger;
using EnjoyCQRS.MessageBus;
using EnjoyCQRS.MetadataProviders;

namespace EnjoyCQRS.EventSource.Storage
{
    public class Session : ISession
    {
        private readonly AggregateTracker _aggregateTracker = new AggregateTracker();
        private readonly List<Aggregate> _aggregates = new List<Aggregate>();
        private readonly IEventStore _eventStore;
        private readonly IEventPublisher _eventPublisher;
        private readonly IProjectionProviderScanner _projectionProviderScanner;
        private readonly IEventUpdateManager _eventUpdateManager;
        private readonly ISnapshotStrategy _snapshotStrategy;
        private readonly IEnumerable<IMetadataProvider> _metadataProviders;
        private readonly IEventsMetadataService _eventsMetadataService;
        private readonly ILogger _logger;

        private bool _externalTransaction;

        public IReadOnlyList<Aggregate> Aggregates => _aggregates.AsReadOnly();

        public Session(
            ILoggerFactory loggerFactory,
            IEventStore eventStore,
            IEventPublisher eventPublisher,
            IProjectionProviderScanner projectionProviderScanner = null,
            IEventUpdateManager eventUpdateManager = null,
            IEnumerable<IMetadataProvider> metadataProviders = null,
            ISnapshotStrategy snapshotStrategy = null,
            IEventsMetadataService eventsMetadataService = null)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            if (metadataProviders == null) metadataProviders = Enumerable.Empty<IMetadataProvider>();

            metadataProviders = metadataProviders.Concat(new IMetadataProvider[]
            {
                new AggregateTypeMetadataProvider(),
                new EventTypeMetadataProvider(),
                new CorrelationIdMetadataProvider()
            });

            if (projectionProviderScanner == null)
            {
                projectionProviderScanner = new ProjectionProviderAttributeScanner();
            }

            if (snapshotStrategy == null)
            {
                snapshotStrategy = new IntervalSnapshotStrategy();
            }

            if (eventsMetadataService == null)
            {
                eventsMetadataService = new EventsMetadataService();
            }

            _logger = loggerFactory.Create(nameof(Session));

            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
            _snapshotStrategy = snapshotStrategy;
            _eventUpdateManager = eventUpdateManager;
            _projectionProviderScanner = projectionProviderScanner;
            _metadataProviders = metadataProviders;
            _eventsMetadataService = eventsMetadataService;
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
            _logger.LogDebug($"Getting aggregate '{typeof(TAggregate).FullName}' with identifier: '{id}'.");

            var aggregate = _aggregateTracker.GetById<TAggregate>(id);

            _logger.LogDebug("Returning an aggregate tracked.");

            if (aggregate != null)
            {
                RegisterForTracking(aggregate);

                return aggregate;
            }

            aggregate = new TAggregate();

            IEnumerable<ICommitedEvent> events;

            _logger.LogDebug("Checking if aggregate has snapshot support.");

            if (_snapshotStrategy.CheckSnapshotSupport(aggregate.GetType()))
            {
                var snapshotAggregate = aggregate as ISnapshotAggregate;
                if (snapshotAggregate != null)
                {
                    int version = 0;
                    var snapshot = await _eventStore.GetLatestSnapshotByIdAsync(id).ConfigureAwait(false);

                    if (snapshot != null)
                    {
                        version = snapshot.AggregateVersion;

                        _logger.LogDebug("Restoring snapshot.");

                        var snapshotRestore = new SnapshotRestore(snapshot.AggregateId, snapshot.AggregateVersion, snapshot.Data, snapshot.Metadata);

                        snapshotAggregate.Restore(snapshotRestore);

                        _logger.LogDebug("Snapshot restored.");
                    }

                    events = await _eventStore.GetEventsForwardAsync(id, version).ConfigureAwait(false);

                    LoadAggregate(aggregate, events);
                }
            }
            else
            {
                events = await _eventStore.GetAllEventsAsync(id).ConfigureAwait(false);

                LoadAggregate(aggregate, events);
            }

            if (aggregate.Id.Equals(Guid.Empty))
            {
                _logger.LogError($"The aggregate ({typeof(TAggregate).FullName} {id}) was not found.");

                throw new AggregateNotFoundException(typeof(TAggregate).Name, id);
            }

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

        /// <summary>
        /// Start transaction.
        /// </summary>
        public void BeginTransaction()
        {
            _logger.LogInformation($"Called method: {nameof(Session)}.{nameof(BeginTransaction)}.");

            if (_externalTransaction)
            {
                _logger.LogError("Transaction already was open.");

                throw new InvalidOperationException("The transaction already was open.");
            }

            _externalTransaction = true;
            _eventStore.BeginTransaction();
        }

        /// <summary>
        /// Confirm changes.
        /// </summary>
        /// <returns></returns>
        public async Task CommitAsync()
        {
            _logger.LogInformation($"Called method: {nameof(Session)}.{nameof(CommitAsync)}.");

            _logger.LogInformation($"Calling method: {_eventStore.GetType().Name}.{nameof(CommitAsync)}.");

            await _eventStore.CommitAsync().ConfigureAwait(false);

            _externalTransaction = false;

            Reset();
        }

        /// <summary>
        /// Call <see cref="IEventStore.SaveAsync"/> in <see cref="IEventStore"/> passing serialized events.
        /// </summary>
        public virtual async Task SaveChangesAsync()
        {
            _logger.LogInformation($"Called method: {nameof(Session)}.{nameof(SaveChangesAsync)}.");

            if (!_externalTransaction)
            {
                _eventStore.BeginTransaction();
            }

            // If transaction called externally, the client should care with transaction.

            try
            {
                _logger.LogInformation("Serializing events.");

                var uncommitedEvents =
                    _aggregates.SelectMany(e => e.UncommitedEvents)
                    .OrderBy(o => o.CreatedAt)
                    .Cast<UncommitedEvent>()
                    .Select(GenerateMetadata)
                    .ToList();
                
                foreach (var uncommitedEvent in uncommitedEvents)
                {
                    _eventsMetadataService.Add(uncommitedEvent.Data, uncommitedEvent.Metadata);
                }

                _logger.LogInformation("Saving events on Event Store.");

                await _eventStore.SaveAsync(uncommitedEvents).ConfigureAwait(false);

                _logger.LogInformation("Begin iterate in collection of aggregate.");

                foreach (var aggregate in _aggregates)
                {
                    _logger.LogInformation($"Checking if should take snapshot for aggregate: '{aggregate.Id}'.");

                    if (_snapshotStrategy.ShouldMakeSnapshot(aggregate))
                    {
                        _logger.LogInformation("Taking aggregate's snapshot.");

                        await aggregate.TakeSnapshot(_eventStore).ConfigureAwait(false);
                    }

                    _logger.LogInformation($"Update aggregate's version from {aggregate.Version} to {aggregate.Sequence}.");

                    aggregate.UpdateVersion(aggregate.Sequence);

                    aggregate.ClearUncommitedEvents();

                    _logger.LogInformation($"Scanning projection providers for {aggregate.GetType().Name}.");

                    var scanResult = await _projectionProviderScanner.ScanAsync(aggregate.GetType()).ConfigureAwait(false);

                    _logger.LogInformation($"Projection providers found: {scanResult.Providers.Count()}.");

                    foreach (var provider in scanResult.Providers)
                    {
                        var projection = provider.CreateProjection(aggregate);
                        
                        await _eventStore.SaveProjectionAsync(projection).ConfigureAwait(false);
                    }

                }

                _logger.Log(LogLevel.Information, "End iterate.");

                _logger.LogInformation($"Publishing events. [Qty: {uncommitedEvents.Count}]");

                await _eventPublisher.PublishAsync(uncommitedEvents.Select(e => e.Data)).ConfigureAwait(false);

                _logger.LogInformation("Published events.");

                //ResetTracking();

                await _eventPublisher.CommitAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, e.Message, e);

                if (!_externalTransaction)
                {
                    Rollback();
                }

                throw;
            }

            if (!_externalTransaction)
            {
                await CommitAsync().ConfigureAwait(false);
            }
        }

        private UncommitedEvent GenerateMetadata(UncommitedEvent uncommitedEvent)
        {
            var metadatas = _metadataProviders.SelectMany(md => md.Provide(uncommitedEvent.Aggregate,
                        uncommitedEvent.Data,
                        MetadataCollection.Empty)).Concat(new[]
                        {
                            new KeyValuePair<string, object>(MetadataKeys.EventId, Guid.NewGuid()),
                            new KeyValuePair<string, object>(MetadataKeys.EventVersion, uncommitedEvent.Version),
                            new KeyValuePair<string, object>(MetadataKeys.Timestamp, DateTime.UtcNow),
                        });

            var metadata = new MetadataCollection(metadatas);

            uncommitedEvent.Metadata = uncommitedEvent.Metadata.Merge(metadata);

            return uncommitedEvent;
        }

        /// <summary>
        /// Rollback <see cref="IEventPublisher"/>, <see cref="IEventStore"/> and remove aggregate tracking.
        /// </summary>
        public void Rollback()
        {
            _logger.LogInformation("Calling Event Publisher Rollback.");

            _eventPublisher.Rollback();

            _logger.LogInformation("Calling Event Store Rollback.");

            _eventStore.Rollback();

            _logger.LogInformation("Cleaning tracker.");

            foreach (var aggregate in _aggregates)
            {
                _aggregateTracker.Remove(aggregate.GetType(), aggregate.Id);
            }

            Reset();
        }

        private void RegisterForTracking<TAggregate>(TAggregate aggregateRoot) where TAggregate : Aggregate
        {
            _logger.LogDebug($"Adding to track: {aggregateRoot.GetType().FullName}.");

            if (_aggregates.All(e => e.Id != aggregateRoot.Id))
            {
                _aggregates.Add(aggregateRoot);
            }

            _aggregateTracker.Add(aggregateRoot);
        }

        private void CheckConcurrency<TAggregate>(TAggregate aggregateRoot) where TAggregate : Aggregate
        {
            _logger.LogDebug("Checking concurrency.");

            var trackedAggregate = _aggregateTracker.GetById<TAggregate>(aggregateRoot.Id);

            if (trackedAggregate == null) return;

            if (trackedAggregate.Version != aggregateRoot.Version)
            {
                _logger.LogError($"Aggregate's current version is: {aggregateRoot.Version} - expected is: {trackedAggregate.Version}.");

                throw new ExpectedVersionException<TAggregate>(aggregateRoot, trackedAggregate.Version);
            }
        }
        
        private void LoadAggregate<TAggregate>(TAggregate aggregate, IEnumerable<ICommitedEvent> commitedEvents) where TAggregate : Aggregate
        {
            var flatten = commitedEvents as ICommitedEvent[] ?? commitedEvents.ToArray();


            if (flatten.Any())
            {
                var events = flatten.Select(e => e.Data).Cast<IDomainEvent>();

                if (_eventUpdateManager != null)
                {
                    _logger.LogDebug("Calling Update Manager");

                    events = _eventUpdateManager.Update(events);
                }

                aggregate.LoadFromHistory(new CommitedEventsCollection(events));

                aggregate.UpdateVersion(flatten.Select(e => e.Version).Max());
            }
        }

        private void Reset()
        {
            _aggregates.Clear();
        }
    }
}