using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EnjoyCQRS.EventStore.MongoDB
{
    public class Event
    {
        [BsonElement("id")]
        public Guid Id { get; set; }

        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; }

        [BsonElement("eventType")]
        public string EventType { get; set; }

        [BsonElement("aggregateId")]
        public Guid AggregateId { get; set; }

        [BsonElement("version")]
        public int Version { get; set; }

        [BsonElement("eventData")]
        public BsonDocument EventData { get; set; }

        [BsonElement("metadata")]
        public BsonDocument Metadata { get; set; }
    }
}