using System;

namespace EnjoyCQRS.EventSource.Snapshots
{
    public class SerializedSnapshot : ISerializedSnapshot
    {
        public Guid AggregateId { get; }
        public int AggregateVersion { get; }
        public string SerializedData { get; }
        public string SerializedMetadata { get; }
        public IMetadata Metadata { get; }

        public SerializedSnapshot(Guid aggregateId, int aggregateVersion, string serializedData, string serializedMetadata, IMetadata metadata)
        {
            AggregateId = aggregateId;
            AggregateVersion = aggregateVersion;
            SerializedData = serializedData;
            SerializedMetadata = serializedMetadata;
            Metadata = metadata;
        }
    }
}