using System;
using EnjoyCQRS.Events;
using MongoDB.Bson;

namespace EnjoyCQRS.EventStore.MongoDB
{
    public class MongoCommitedEvent : ICommitedEvent
    {
        public Guid AggregateId { get; private set; }
        public int AggregateVersion { get; private set; }
        public string SerializedData { get; private set; }
        public string SerializedMetadata { get; private set; }

        public static MongoCommitedEvent Create(Event e)
        {
            return new MongoCommitedEvent
            {

                AggregateId = e.AggregateId,
                AggregateVersion = e.Version,
                SerializedData = e.EventData.ToJson(),
                SerializedMetadata = e.Metadata.ToJson(),
            };
        }
    }
}