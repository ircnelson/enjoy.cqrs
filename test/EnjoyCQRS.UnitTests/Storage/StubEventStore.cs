using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnjoyCQRS.Collections;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.EventSource.Storage;

namespace EnjoyCQRS.UnitTests.Storage
{
    public class StubEventStore : IEventStore
    {
        public readonly Dictionary<Guid, List<IDomainEvent>> EventStore = new Dictionary<Guid, List<IDomainEvent>>();
        public readonly ConcurrentDictionary<Guid, List<ISnapshot>> SnapshotStore = new ConcurrentDictionary<Guid, List<ISnapshot>>();

        public bool SaveSnapshotMethodCalled { get; private set; }
        public bool GetSnapshotMethodCalled { get; private set; }

        public bool MakeSnapshot { get; set; } = true;

        public bool InTransaction;

        public void BeginTransaction()
        {
            InTransaction = true;
        }

        public Task CommitAsync()
        {
            if (!InTransaction) throw new InvalidOperationException("You are not in transaction.");

            InTransaction = false;

            return Task.CompletedTask;
        }

        public void Rollback()
        {
            InTransaction = false;
        }

        public Task<IEnumerable<IDomainEvent>> GetAllEventsAsync(Guid id)
        {
            if (EventStore.ContainsKey(id))
            {
                var events = EventStore[id];

                return Task.FromResult(events.AsEnumerable());
            }

            return Task.FromResult(Enumerable.Empty<IDomainEvent>());
        }
        
        public Task SaveAsync(UncommitedDomainEventCollection collection)
        {
            List<IDomainEvent> list;
            if (!EventStore.TryGetValue(collection.AggregateMetadata.Id, out list))
            {
                list = new List<IDomainEvent>();
                EventStore.Add(collection.AggregateMetadata.Id, list); 
            }

            list.AddRange(collection);
            
            return Task.CompletedTask;
        }


        public Task SaveSnapshotAsync<TSnapshot>(TSnapshot snapshot) where TSnapshot : ISnapshot
        {
            SaveSnapshotMethodCalled = true;

            if (!MakeSnapshot) return Task.CompletedTask;

            var snapshots = new List<ISnapshot>();
            SnapshotStore.GetOrAdd(snapshot.AggregateId, snapshots);
            snapshots.Add(snapshot);

            return Task.CompletedTask;
        }

        public Task<ISnapshot> GetSnapshotByIdAsync(Guid aggregateId)
        {
            GetSnapshotMethodCalled = true;

            var snapshot = SnapshotStore[aggregateId].OrderBy(o => o.Version).LastOrDefault();

            return Task.FromResult(snapshot);
        }

        public Task<IEnumerable<IDomainEvent>> GetEventsForwardAsync(Guid aggregateId, int version)
        {
            var events = EventStore[aggregateId].Where(e => e.Version > version);

            return Task.FromResult(events);
        }
        public void Dispose()
        {
        }
    }
}