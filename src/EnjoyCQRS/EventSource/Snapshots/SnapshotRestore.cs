using System;

namespace EnjoyCQRS.EventSource.Snapshots
{
    public class SnapshotRestore : ISnapshotRestore
    {
        public Guid AggregateId { get; }
        public int AggregateVersion { get; }
        public ISnapshot Snapshot { get; }
        public IMetadata Metadata { get; }

        public SnapshotRestore(Guid aggregateId, int aggregateVersion, ISnapshot snapshot, IMetadata metadata)
        {
            AggregateId = aggregateId;
            AggregateVersion = aggregateVersion;
            Snapshot = snapshot;
            Metadata = metadata;
        }
    }
}