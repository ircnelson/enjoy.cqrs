using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnjoyCQRS.Collections;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.EventSource.Storage;

namespace EnjoyCQRS.Owin.IntegrationTests.Infrastructure
{
    public class StubEventStore : IEventStore
    {
        public readonly ConcurrentDictionary<Guid, List<IDomainEvent>> Events = new ConcurrentDictionary<Guid, List<IDomainEvent>>();
        public readonly ConcurrentDictionary<Guid, List<ISnapshot>> Snapshots = new ConcurrentDictionary<Guid, List<ISnapshot>>();

        private readonly ConcurrentDictionary<Guid, UncommitedDomainEventCollection> _uncommitedEvents = new ConcurrentDictionary<Guid, UncommitedDomainEventCollection>();

        private readonly List<ISnapshot> _uncommitedSnapshots = new List<ISnapshot>();
        
        public bool InTransaction;

        public Task SaveSnapshotAsync<TSnapshot>(TSnapshot snapshot) where TSnapshot : ISnapshot
        {
            _uncommitedSnapshots.Add(snapshot);

            return Task.CompletedTask;
        }

        public Task<ISnapshot> GetSnapshotByIdAsync(Guid aggregateId)
        {
            List<ISnapshot> snapshots;
            if (Snapshots.TryGetValue(aggregateId, out snapshots))
            {
                var snapshot = snapshots.OrderByDescending(e => e.Version).Take(1).FirstOrDefault();

                return Task.FromResult(snapshot);
            }

            return Task.FromResult<ISnapshot>(null);
        }

        public Task<IEnumerable<IDomainEvent>> GetEventsForwardAsync(Guid aggregateId, int version)
        {
            List<IDomainEvent> events;
            if (Events.TryGetValue(aggregateId, out events))
            {
                var forwardEvents = events.Where(e => e.Version > version).OrderBy(e => e.Version);

                return Task.FromResult<IEnumerable<IDomainEvent>>(forwardEvents);
            }

            return Task.FromResult<IEnumerable<IDomainEvent>>(null);
        }

        public void Dispose()
        {
        }

        public void BeginTransaction()
        {
            InTransaction = true;
        }

        public Task CommitAsync()
        {
            if (!InTransaction) throw new InvalidOperationException("You are not in transaction.");

            InTransaction = false;

            var groupedEvents = _uncommitedEvents.Values.GroupBy(e => e.AggregateMetadata.Id).Select(e => new { AggregateId = e.Key, Events = e });

            foreach (var uncommitedEvent in groupedEvents)
            {
                List<IDomainEvent> events;

                if (!Events.TryGetValue(uncommitedEvent.AggregateId, out events))
                {
                    events = new List<IDomainEvent>();
                }

                events.AddRange(uncommitedEvent.Events.SelectMany(e => e));

                Events[uncommitedEvent.AggregateId] = events;
            }

            _uncommitedEvents.Clear();

            var snapshotGrouped = _uncommitedSnapshots.GroupBy(e => e.AggregateId).Select(e => new { AggregateId = e.Key, Snapshots = e });

            foreach (var snapshot in snapshotGrouped)
            {
                List<ISnapshot> snapshots;
                if (!Snapshots.TryGetValue(snapshot.AggregateId, out snapshots))
                {
                    snapshots = new List<ISnapshot>();
                }

                snapshots.AddRange(snapshot.Snapshots);

                Snapshots[snapshot.AggregateId] = snapshots;
            }

            _uncommitedSnapshots.Clear();

            return Task.CompletedTask;
        }

        public void Rollback()
        {
            _uncommitedEvents.Clear();
            _uncommitedSnapshots.Clear();

            InTransaction = false;
        }

        public Task<IEnumerable<IDomainEvent>> GetAllEventsAsync(Guid id)
        {
            var events = Events[id].OrderBy(e => e.Version).ToList();

            return Task.FromResult<IEnumerable<IDomainEvent>>(events);
        }

        public Task SaveAsync(UncommitedDomainEventCollection collection)
        {
            if (_uncommitedEvents.TryAdd(collection.AggregateMetadata.Id, collection))
            {
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }
    }
}