using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource.Snapshots;

namespace EnjoyCQRS.EventSource.Storage
{
    public class InMemoryEventStore : IEventStore
    {
        public IReadOnlyList<ICommitedEvent> Events => _events.AsReadOnly();
        public IReadOnlyList<ISnapshot> Snapshots => _snapshots.AsReadOnly();

        private readonly List<ICommitedEvent> _events = new List<ICommitedEvent>();
        private readonly List<ISnapshot> _snapshots = new List<ISnapshot>();

        private readonly List<ISerializedEvent> _uncommitedEvents = new List<ISerializedEvent>();

        private readonly List<ISnapshot> _uncommitedSnapshots = new List<ISnapshot>();

        public bool InTransaction;
        
        public virtual Task SaveSnapshotAsync<TSnapshot>(TSnapshot snapshot) where TSnapshot : ISnapshot
        {
            _uncommitedSnapshots.Add(snapshot);

            return Task.CompletedTask;
        }

        public virtual Task<ISnapshot> GetSnapshotByIdAsync(Guid aggregateId)
        {
            var snapshot = Snapshots.Where(e => e.AggregateId == aggregateId).OrderByDescending(e => e.Version).Take(1).FirstOrDefault();

            return Task.FromResult(snapshot);
        }

        public virtual Task<IEnumerable<ICommitedEvent>> GetEventsForwardAsync(Guid aggregateId, int version)
        {
            var events = Events
            .Where(e => e.AggregateId == aggregateId && e.AggregateVersion > version)
            .OrderBy(e => e.AggregateVersion)
            .ToList();

            return Task.FromResult<IEnumerable<ICommitedEvent>>(events);
        }

        public void Dispose()
        {
        }

        public void BeginTransaction()
        {
            InTransaction = true;
        }

        public virtual Task CommitAsync()
        {
            if (!InTransaction) throw new InvalidOperationException("You are not in transaction.");

            InTransaction = false;

            _events.AddRange(_uncommitedEvents.Select(InstantiateCommitedEvent));

            _uncommitedEvents.Clear();

            var snapshotGrouped = _uncommitedSnapshots.GroupBy(e => e.AggregateId).Select(e => new { AggregateId = e.Key, Snapshots = e });

            foreach (var snapshot in snapshotGrouped)
            {
                _snapshots.AddRange(snapshot.Snapshots);
            }

            _uncommitedSnapshots.Clear();

            return Task.CompletedTask;
        }

        public virtual void Rollback()
        {
            _uncommitedEvents.Clear();
            _uncommitedSnapshots.Clear();

            InTransaction = false;
        }

        public virtual Task<IEnumerable<ICommitedEvent>> GetAllEventsAsync(Guid id)
        {
            var events = Events
            .Where(e => e.AggregateId == id)
            .OrderBy(e => e.AggregateVersion)
            .ToList();

            return Task.FromResult<IEnumerable<ICommitedEvent>>(events);
        }

        public virtual Task SaveAsync(IEnumerable<ISerializedEvent> collection)
        {
            _uncommitedEvents.AddRange(collection);

            return Task.CompletedTask;
        }

        private static ICommitedEvent InstantiateCommitedEvent(ISerializedEvent serializedEvent)
        {
            return new InMemoryCommitedEvent(serializedEvent.AggregateId, serializedEvent.AggregateVersion, serializedEvent.SerializedData, serializedEvent.SerializedMetadata);
        }

        internal class InMemoryCommitedEvent : ICommitedEvent
        {
            public InMemoryCommitedEvent(Guid aggregateId, int aggregateVersion, string serializedData, string serializedMetadata)
            {
                AggregateId = aggregateId;
                AggregateVersion = aggregateVersion;
                SerializedData = serializedData;
                SerializedMetadata = serializedMetadata;
            }

            public Guid AggregateId { get; }
            public int AggregateVersion { get; }
            public string SerializedData { get; }
            public string SerializedMetadata { get; }
        }
    }
}