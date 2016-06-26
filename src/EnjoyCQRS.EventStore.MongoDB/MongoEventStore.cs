using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.EventSource.Storage;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace EnjoyCQRS.EventStore.MongoDB
{
    public class MongoEventStore : IEventStore
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };

        private readonly MongoClient _client;
        private readonly List<Event> _uncommitedEvents = new List<Event>();
        private readonly List<SnapshotData> _uncommitedSnapshots = new List<SnapshotData>();
        private readonly Dictionary<string, Type> _typeCache = new Dictionary<string, Type>();

        private const string EventsCollectionName = "Events";
        private const string SnapshotsCollectionName = "Snapshots";

        public string Database { get; }

        public MongoEventStore(MongoClient client, string database)
        {
            Database = database;
            _client = client;
        }

        public Task SaveSnapshotAsync(ISerializedSnapshot snapshot)
        {
            var snapshotData = Serialize(snapshot);
            _uncommitedSnapshots.Add(snapshotData);

            return Task.CompletedTask;
        }

        public async Task<ICommitedSnapshot> GetLatestSnapshotByIdAsync(Guid aggregateId)
        {
            var db = _client.GetDatabase(Database);
            var snapshotCollection = db.GetCollection<SnapshotData>(SnapshotsCollectionName);

            var filter = Builders<SnapshotData>.Filter;
            var sort = Builders<SnapshotData>.Sort;

            var snapshots = await snapshotCollection
                .Find(filter.Eq(x => x.AggregateId, aggregateId))
                .Sort(sort.Descending(x => x.Version))
                .Limit(1)
                .ToListAsync();

            return snapshots.Select(Deserialize).FirstOrDefault();
        }

        public async Task<IEnumerable<ICommitedEvent>> GetEventsForwardAsync(Guid aggregateId, int version)
        {
            var db = _client.GetDatabase(Database);
            var collection = db.GetCollection<Event>(EventsCollectionName);

            var filter = Builders<Event>.Filter;
            var sort = Builders<Event>.Sort;

            var events = await collection
                .Find(filter.Eq(x => x.AggregateId, aggregateId) & filter.Gt(x => x.Version, version))
                .Sort(sort.Ascending(x => x.Version))
                .ToListAsync();

            return events.Select(Deserialize).ToList();
        }

        public void Dispose()
        {
        }

        public void BeginTransaction()
        {
        }

        public async Task CommitAsync()
        {
            if (_uncommitedEvents.Count == 0) return;

            var db = _client.GetDatabase(Database);
            var eventCollection = db.GetCollection<Event>(EventsCollectionName);
            var snapshotCollection = db.GetCollection<SnapshotData>(SnapshotsCollectionName);

            if (_uncommitedSnapshots.Count > 0)
                await snapshotCollection.InsertManyAsync(_uncommitedSnapshots);

            if (_uncommitedEvents.Count > 0)
                await eventCollection.InsertManyAsync(_uncommitedEvents);

            _uncommitedEvents.Clear();
            _uncommitedSnapshots.Clear();
        }

        public void Rollback()
        {
            _uncommitedEvents.Clear();
            _uncommitedSnapshots.Clear();
        }

        public async Task<IEnumerable<ICommitedEvent>> GetAllEventsAsync(Guid id)
        {
            var db = _client.GetDatabase(Database);
            var collection = db.GetCollection<Event>(EventsCollectionName);

            var events = await collection
                .Find(Builders<Event>.Filter.Eq(x => x.AggregateId, id))
                .Sort(Builders<Event>.Sort.Ascending(x => x.Version))
                .ToListAsync();

            return events.Select(Deserialize).ToList();
        }

        public Task SaveAsync(IEnumerable<ISerializedEvent> collection)
        {
            var eventList = collection.Select(Serialize);
            _uncommitedEvents.AddRange(eventList);

            return Task.CompletedTask;
        }

        private Event Serialize(ISerializedEvent serializedEvent)
        {
            var eventData = BsonDocument.Parse(serializedEvent.SerializedData);
            var metadata = BsonDocument.Parse(serializedEvent.SerializedMetadata);
            var id = serializedEvent.Metadata.GetValue(MetadataKeys.EventId, Guid.Parse);
            var eventType = serializedEvent.Metadata.GetValue(MetadataKeys.EventName);
            
            var @event = new Event
            {
                Id = id,
                Timestamp = DateTime.UtcNow,
                EventType = eventType,
                AggregateId = serializedEvent.AggregateId,
                Version = serializedEvent.AggregateVersion,
                EventData = eventData,
                Metadata = metadata
            };

            return @event;
        }

        private ICommitedEvent Deserialize(Event e)
        {
            return MongoCommitedEvent.Create(e);
        }

        private ICommitedSnapshot Deserialize(SnapshotData snapshotData)
        {
            return MongoCommitedSnapshot.Create(snapshotData);
        }

        private SnapshotData Serialize(ISerializedSnapshot serializedSnapshot)
        {
            var eventData = BsonDocument.Parse(serializedSnapshot.SerializedData);
            var metadata = BsonDocument.Parse(serializedSnapshot.SerializedMetadata);
            var id = serializedSnapshot.Metadata.GetValue(MetadataKeys.SnapshotId, Guid.Parse);
            
            var snapshot = new SnapshotData
            {
                Id = id,
                Timestamp = DateTime.UtcNow,
                AggregateId = serializedSnapshot.AggregateId,
                Version = serializedSnapshot.AggregateVersion,
                Data = eventData,
                Metadata = metadata,
            };

            return snapshot;
        }

        private Type GetOrAddTypeCache(string typeName)
        {
            Type type;
            if (!_typeCache.TryGetValue(typeName, out type))
            {
                type = Type.GetType(typeName);
                _typeCache.Add(typeName, type);
            }

            return type;
        }
    }
}
