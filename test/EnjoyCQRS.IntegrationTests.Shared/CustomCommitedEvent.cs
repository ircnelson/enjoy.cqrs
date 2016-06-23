using System;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.IntegrationTests.Shared
{
    public class CustomCommitedEvent : ICommitedEvent
    {
        public Guid AggregateId { get; }
        public int AggregateVersion { get; }
        public string SerializedData { get; }
        public string SerializedMetadata { get; }

        public CustomCommitedEvent(Guid aggregateId, int aggregateVersion, string serializedData, string serializedMetadata)
        {
            AggregateId = aggregateId;
            AggregateVersion = aggregateVersion;
            SerializedData = serializedData;
            SerializedMetadata = serializedMetadata;
        }

    }
}