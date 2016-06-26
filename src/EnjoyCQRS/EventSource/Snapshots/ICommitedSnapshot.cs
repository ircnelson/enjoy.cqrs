using System;

namespace EnjoyCQRS.EventSource.Snapshots
{
    public interface ICommitedSnapshot
    {
        Guid AggregateId { get; }
        int AggregateVersion { get; }
        string SerializedData { get; }
        string SerializedMetadata { get; }
    }
}