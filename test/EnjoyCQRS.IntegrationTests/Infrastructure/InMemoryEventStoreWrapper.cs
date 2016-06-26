using System;
using System.Threading.Tasks;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.EventSource.Storage;

namespace EnjoyCQRS.IntegrationTests.Infrastructure
{
    public class InMemoryEventStoreWrapper : InMemoryEventStore
    {
        public bool GetSnapshotCalled { get; private set; }
        public bool SaveSnapshotCalled { get; private set; }

        public override Task SaveSnapshotAsync(ISerializedSnapshot snapshot)
        {
            SaveSnapshotCalled = true;

            return base.SaveSnapshotAsync(snapshot);
        }

        public override Task<ICommitedSnapshot> GetLatestSnapshotByIdAsync(Guid aggregateId)
        {
            GetSnapshotCalled = true;

            return base.GetLatestSnapshotByIdAsync(aggregateId);
        }
    }
}