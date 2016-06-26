using System;

namespace EnjoyCQRS.EventSource.Snapshots
{
    public interface ISerializedSnapshot
    {
        Guid AggregateId { get; }
        int AggregateVersion { get; }
        string SerializedData { get; }
        string SerializedMetadata { get; }
        IMetadata Metadata { get; }
    }
}