using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.UnitTests.Storage
{
    public class StubEventStore : InMemoryEventStore
    {
        public bool SaveSnapshotMethodCalled { get; private set; }
        public bool GetSnapshotMethodCalled { get; private set; }
        
        public override Task SaveAsync(IEnumerable<IUncommittedEvent> collection)
        {
            SaveSnapshotMethodCalled = true;

            return base.SaveAsync(collection);
        }

        public override Task<ICommittedSnapshot> GetLatestSnapshotByIdAsync(Guid aggregateId)
        {
            GetSnapshotMethodCalled = true;

            return base.GetLatestSnapshotByIdAsync(aggregateId);
        }
    }
}