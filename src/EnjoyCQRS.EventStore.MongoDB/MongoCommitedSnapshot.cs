using System;
using EnjoyCQRS.EventSource.Snapshots;
using MongoDB.Bson;

namespace EnjoyCQRS.EventStore.MongoDB
{
    public class MongoCommitedSnapshot : ICommitedSnapshot
    {
        public Guid AggregateId { get; private set; }
        public int AggregateVersion { get; private set; }
        public string SerializedData { get; private set; }
        public string SerializedMetadata { get; private set; }

        public static MongoCommitedSnapshot Create(SnapshotData e)
        {
            return new MongoCommitedSnapshot
            {

                AggregateId = e.AggregateId,
                AggregateVersion = e.Version,
                SerializedData = e.Data.ToJson(),
                SerializedMetadata = e.Metadata.ToJson(),
            };
        }
    }
}