// The MIT License (MIT)
// 
// Copyright (c) 2016 Nelson Corrêa V. Júnior
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Projections;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.EventSource.Storage;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using MongoDB.Bson.Serialization;
using EnjoyCQRS.Collections;

namespace EnjoyCQRS.EventStore.MongoDB
{
    public class MongoEventStore : IEventStore
    {
        public JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        protected readonly List<IUncommittedEvent> UncommittedEvents = new List<IUncommittedEvent>();
        protected readonly List<IUncommittedSnapshot> UncommittedSnapshots = new List<IUncommittedSnapshot>();
        protected readonly List<IProjection> UncommittedProjections = new List<IProjection>();

        public IMongoClient Client { get; }
        public string Database { get; }
        public MongoEventStoreSetttings Setttings { get; }

        public MongoEventStore(IMongoDatabase database) : this (database, new MongoEventStoreSetttings())
        {
        }

        public MongoEventStore(IMongoDatabase database, MongoEventStoreSetttings setttings)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            if (setttings == null) throw new ArgumentNullException(nameof(setttings));

            setttings.Validate();

            Database = database.DatabaseNamespace.DatabaseName;
            Setttings = setttings;
            Client = database.Client;
        }

        public MongoEventStore(MongoClient client, string database) : this(client, database, new MongoEventStoreSetttings())
        {
        }

        public MongoEventStore(MongoClient client, string database, MongoEventStoreSetttings setttings = null)
        {
            if (setttings == null) throw new ArgumentNullException(nameof(setttings));

            setttings.Validate();

            Database = database ?? throw new ArgumentNullException(nameof(database));
            Client = client ?? throw new ArgumentNullException(nameof(client));
            Setttings = setttings;
        }

        public Task SaveSnapshotAsync(IUncommittedSnapshot snapshot)
        {
            UncommittedSnapshots.Add(snapshot);

            return Task.CompletedTask;
        }

        public async Task<ICommittedSnapshot> GetLatestSnapshotByIdAsync(Guid aggregateId)
        {
            var db = Client.GetDatabase(Database);
            var snapshotCollection = db.GetCollection<SnapshotDocument>(Setttings.SnapshotsCollectionName);

            var filter = Builders<SnapshotDocument>.Filter;
            var sort = Builders<SnapshotDocument>.Sort;

            var snapshots = await snapshotCollection
                .Find(filter.Eq(x => x.AggregateId, aggregateId))
                .Sort(sort.Descending(x => x.Version))
                .Limit(1)
                .ToListAsync();

            return snapshots.Select(Deserialize).FirstOrDefault();
        }

        public Task<IEnumerable<ICommittedEvent>> GetEventsForwardAsync(Guid aggregateId, int version)
        {
            var db = Client.GetDatabase(Database);
            var collection = db.GetCollection<EventDocument>(Setttings.EventsCollectionName);

            var sort = Builders<EventDocument>.Sort;
            var filterBuilder = Builders<EventDocument>.Filter;

            var filter = filterBuilder.Empty
                & filterBuilder.Eq(x => x.AggregateId, aggregateId)
                & filterBuilder.Gt(x => x.Version, version)
                & filterBuilder.Or(filterBuilder.Exists(x => x.Metadata[MetadataKeys.EventIgnore], exists: false), filterBuilder.Eq(x => x.Metadata[MetadataKeys.EventIgnore], false));
            
            var events = collection
                .Find(filter)
                .Sort(sort.Ascending(x => x.Metadata[MetadataKeys.EventVersion]))
                .ToList()
                .Select(Deserialize);

            return Task.FromResult(events);
        }

        public void Dispose()
        {
        }

        public void BeginTransaction()
        {
        }

        public async Task CommitAsync()
        {
            var db = Client.GetDatabase(Database);
            
            if (UncommittedSnapshots.Count > 0)
            {
                var snapshotCollection = db.GetCollection<SnapshotDocument>(Setttings.SnapshotsCollectionName);

                var serializedSnapshots = UncommittedSnapshots.Select(Serialize);

                await snapshotCollection.InsertManyAsync(serializedSnapshots);
            }

            if (UncommittedEvents.Count > 0)
            {
                var eventCollection = db.GetCollection<EventDocument>(Setttings.EventsCollectionName);

                var serializedEvents = UncommittedEvents.Select(Serialize);

                await eventCollection.InsertManyAsync(serializedEvents);
            }

            if (UncommittedProjections.Count > 0)
            {
                await AddOrUpdateProjectionsAsync(UncommittedProjections);
            }

            Cleanup();
        }

        public void Rollback()
        {
            Cleanup();
        }

        public async Task<IEnumerable<ICommittedEvent>> GetAllEventsAsync(Guid id)
        {
            var db = Client.GetDatabase(Database);
            var collection = db.GetCollection<EventDocument>(Setttings.EventsCollectionName);

            var sort = Builders<EventDocument>.Sort;
            var filterBuilder = Builders<EventDocument>.Filter;

            var filter = filterBuilder.Empty 
                & filterBuilder.Eq(x => x.AggregateId, id)
                & filterBuilder.Or(filterBuilder.Exists(x => x.Metadata[MetadataKeys.EventIgnore], exists: false), filterBuilder.Eq(x => x.Metadata[MetadataKeys.EventIgnore], false));

            var events = await collection
                .Find(filter)
                .Sort(sort.Ascending(x => x.Metadata[MetadataKeys.EventVersion]))
                .ToListAsync();

            return events.Select(Deserialize).ToList();
        }

        public Task SaveAsync(IEnumerable<IUncommittedEvent> collection)
        {
            UncommittedEvents.AddRange(collection);

            return Task.CompletedTask;
        }

        public Task SaveProjectionAsync(IProjection projection)
        {
            UncommittedProjections.Add(projection);

            return Task.CompletedTask;
        }

        private EventDocument Serialize(IUncommittedEvent eventToSerialize)
        {
            var id = eventToSerialize.Metadata.GetValue(MetadataKeys.EventId, value => Guid.Parse(value.ToString()));
            var eventType = eventToSerialize.Metadata.GetValue(MetadataKeys.EventName, value => value.ToString());
            var timestamp = eventToSerialize.Metadata.GetValue(MetadataKeys.Timestamp, value => (DateTime)value);

            var json = JsonConvert.SerializeObject(eventToSerialize.Data, JsonSettings);

            var @event = new EventDocument
            {
                Id = id,
                Timestamp = timestamp,
                EventType = eventType,
                AggregateId = eventToSerialize.AggregateId,
                Version = eventToSerialize.Version,
                EventData = BsonDocument.Parse(json),
                Metadata = BsonDocument.Create(eventToSerialize.Metadata)
            };

            return @event;
        }

        private ICommittedEvent Deserialize(EventDocument doc)
        {
            var eventType = Type.GetType(doc.Metadata[MetadataKeys.EventClrType].AsString);
            var data = JsonConvert.DeserializeObject(doc.EventData.ToJson(), eventType, JsonSettings);
            
            var metadata = BsonSerializer.Deserialize<Dictionary<string, object>>(doc.Metadata.ToJson());
            var version = doc.Metadata[MetadataKeys.EventVersion].AsInt32;

            return new MongoCommittedEvent(doc.AggregateId, version, data, new MetadataCollection(metadata));
        }

        private ICommittedSnapshot Deserialize(SnapshotDocument doc)
        {
            var snapshotType = Type.GetType(doc.Metadata[MetadataKeys.SnapshotClrType].AsString);
            
            var data = (ISnapshot) BsonSerializer.Deserialize(doc.Data, snapshotType);
            var metadata = BsonSerializer.Deserialize<Dictionary<string, object>>(doc.Metadata.ToJson());
            
            return new MongoCommittedSnapshot(doc.AggregateId, doc.Version, data, new MetadataCollection(metadata));
        }

        private SnapshotDocument Serialize(IUncommittedSnapshot serializedSnapshot)
        {
            var id = serializedSnapshot.Metadata.GetValue(MetadataKeys.SnapshotId, value => Guid.Parse(value.ToString()));
            var data = BsonDocumentWrapper.Create(serializedSnapshot.Data);
            var metadata = BsonDocument.Create(serializedSnapshot.Metadata);

            var snapshot = new SnapshotDocument
            {
                Id = id,
                Timestamp = DateTime.UtcNow,
                AggregateId = serializedSnapshot.AggregateId,
                Version = serializedSnapshot.AggregateVersion,
                Data = data,
                Metadata = metadata,
            };

            return snapshot;
        }

        private void Cleanup()
        {
            UncommittedEvents.Clear();
            UncommittedSnapshots.Clear();
            UncommittedProjections.Clear();
        }

        protected virtual async Task AddOrUpdateProjectionsAsync(IEnumerable<IProjection> uncommittedProjections)
        {
            var db = Client.GetDatabase(Database);
            var collection = db.GetCollection<BsonDocument>(Setttings.ProjectionsCollectionName);

            var filterBuilder = Builders<BsonDocument>.Filter;

            foreach (var uncommittedProjection in uncommittedProjections)
            {
                var category = uncommittedProjection.GetType().Name;

                var filter = FilterDefinition<BsonDocument>.Empty
                             & filterBuilder.Eq(e => e["_id"], uncommittedProjection.Id)
                             & filterBuilder.Eq(e => e["_t"], category);

                var doc = BsonDocumentWrapper.Create(uncommittedProjection);

                await collection.FindOneAndReplaceAsync(filter, doc, new FindOneAndReplaceOptions<BsonDocument>
                {
                    IsUpsert = true
                });
            }
        }
    }
}