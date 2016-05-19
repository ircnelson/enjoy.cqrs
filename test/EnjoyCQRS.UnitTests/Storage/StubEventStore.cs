using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.EventSource.Storage;

namespace EnjoyCQRS.UnitTests.Storage
{
    public class StubEventStore : IEventStore
    {
        public readonly Dictionary<Guid, List<IDomainEvent>> EventStore = new Dictionary<Guid, List<IDomainEvent>>();
        public readonly ConcurrentDictionary<Guid, List<ISnapshot>> SnapshotStore = new ConcurrentDictionary<Guid, List<ISnapshot>>();

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
        
        public Task SaveAsync(IEnumerable<IDomainEvent> events)
        {
            foreach (var @event in events)
            {
                var aggregateId = @event.AggregateId;

                List<IDomainEvent> list;
                EventStore.TryGetValue(aggregateId, out list);
                if (list == null)
                {
                    list = new List<IDomainEvent>();
                    EventStore.Add(aggregateId, list);
                }
                list.Add(@event);
            }

            return Task.CompletedTask;
            
            //else
            //{
            //    var existingEvents = EventStore[aggregateId];
            //    var currentversion = existingEvents.Count;

            //    if (currentversion != expectedVersion)
            //    {
            //        throw new WrongExpectedVersionException($"Expected version {expectedVersion} but the version is {currentversion}");
            //    }

            //    existingEvents.AddRange(events);
            //}
        }


        public Task SaveSnapshotAsync<TSnapshot>(TSnapshot snapshot) where TSnapshot : ISnapshot
        {
            List<ISnapshot> snapshots = new List<ISnapshot>();
            SnapshotStore.GetOrAdd(snapshot.AggregateId, snapshots);
            snapshots.Add(snapshot);

            return Task.CompletedTask;
        }

        public Task<TSnapshot> GetSnapshotByIdAsync<TSnapshot>(Guid aggregateId) where TSnapshot : ISnapshot
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IDomainEvent>> GetEventsForwardAsync(Guid id, int version)
        {
            throw new NotImplementedException();
        }
        public void Dispose()
        {
        }
    }
}