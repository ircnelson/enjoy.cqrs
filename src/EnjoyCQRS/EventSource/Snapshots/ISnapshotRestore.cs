using System;

namespace EnjoyCQRS.EventSource.Snapshots
{
    public interface ISnapshotRestore
    {
        Guid AggregateId { get; }
        int AggregateVersion { get; }
        ISnapshot Snapshot { get; }
    }
}