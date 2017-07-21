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

        protected readonly List<EventDocument> UncommitedEvents = new List<EventDocument>();
        protected readonly List<SnapshotDocument> UncommitedSnapshots = new List<SnapshotDocument>();
        protected readonly List<IProjection> UncommitedProjections = new List<IProjection>();

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
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (database == null) throw new ArgumentNullException(nameof(database));
            if (setttings == null) throw new ArgumentNullException(nameof(setttings));

            setttings.Validate();

            Database = database;
            Setttings = setttings;
            Client = client;
        }

        public Task SaveSnapshotAsync(IUncommitedSnapshot snapshot)
        {
            var snapshotData = Serialize(snapshot);
            UncommitedSnapshots.Add(snapshotData);

            return Task.CompletedTask;
        }

        public async Task<ICommitedSnapshot> GetLatestSnapshotByIdAsync(Guid aggregateId)
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

        public async Task<IEnumerable<ICommitedEvent>> GetEventsForwardAsync(Guid aggregateId, int version)
        {
            var db = Client.GetDatabase(Database);
            var collection = db.GetCollection<EventDocument>(Setttings.EventsCollectionName);

            var sort = Builders<EventDocument>.Sort;
            var filterBuilder = Builders<EventDocument>.Filter;

            var filter = filterBuilder.Empty
                & filterBuilder.Eq(x => x.AggregateId, aggregateId)
                & filterBuilder.Gt(x => x.Version, version)
                & filterBuilder.Or(filterBuilder.Exists(x => x.Metadata[MetadataKeys.EventIgnore], exists: false), filterBuilder.Eq(x => x.Metadata[MetadataKeys.EventIgnore], false));
            
            var events = await collection
                .Find(filter)
                .Sort(sort.Ascending(x => x.Metadata[MetadataKeys.EventVersion]))
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
            var db = Client.GetDatabase(Database);
            
            if (UncommitedSnapshots.Count > 0)
            {
                var snapshotCollection = db.GetCollection<SnapshotDocument>(Setttings.SnapshotsCollectionName);
                await snapshotCollection.InsertManyAsync(UncommitedSnapshots);
            }

            if (UncommitedEvents.Count > 0)
            {
                var eventCollection = db.GetCollection<EventDocument>(Setttings.EventsCollectionName);
                await eventCollection.InsertManyAsync(UncommitedEvents);
            }

            if (UncommitedProjections.Count > 0)
            {
                await AddOrUpdateProjectionsAsync(UncommitedProjections);
            }

            Cleanup();
        }

        public void Rollback()
        {
            Cleanup();
        }

        public async Task<IEnumerable<ICommitedEvent>> GetAllEventsAsync(Guid id)
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

        public Task SaveAsync(IEnumerable<IUncommitedEvent> collection)
        {
            var eventList = collection.Select(Serialize);
            UncommitedEvents.AddRange(eventList);

            return Task.CompletedTask;
        }

        public Task SaveProjectionAsync(IProjection projection)
        {
            UncommitedProjections.Add(projection);

            return Task.CompletedTask;
        }

        private EventDocument Serialize(IUncommitedEvent eventToSerialize)
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

        private ICommitedEvent Deserialize(EventDocument doc)
        {
            var eventType = Type.GetType(doc.Metadata[MetadataKeys.EventClrType].AsString);
            var data = JsonConvert.DeserializeObject(doc.EventData.ToJson(), eventType, JsonSettings);
            
            var metadata = BsonSerializer.Deserialize<Dictionary<string, object>>(doc.Metadata.ToJson());
            var version = doc.Metadata[MetadataKeys.EventVersion].AsInt32;

            return new MongoCommitedEvent(doc.AggregateId, version, data, new MetadataCollection(metadata));
        }

        private ICommitedSnapshot Deserialize(SnapshotDocument doc)
        {
            var snapshotType = Type.GetType(doc.Metadata[MetadataKeys.SnapshotClrType].AsString);
            
            var data = (ISnapshot) BsonSerializer.Deserialize(doc.Data, snapshotType);
            var metadata = BsonSerializer.Deserialize<Dictionary<string, object>>(doc.Metadata.ToJson());
            
            return new MongoCommitedSnapshot(doc.AggregateId, doc.Version, data, new MetadataCollection(metadata));
        }

        private SnapshotDocument Serialize(IUncommitedSnapshot serializedSnapshot)
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
            UncommitedEvents.Clear();
            UncommitedSnapshots.Clear();
            UncommitedProjections.Clear();
        }

        protected virtual async Task AddOrUpdateProjectionsAsync(IEnumerable<IProjection> serializedProjections)
        {
            var db = Client.GetDatabase(Database);
            var projectionCollection = db.GetCollection<BsonDocument>(Setttings.ProjectionsCollectionName);

            var filterBuilder = Builders<BsonDocument>.Filter;

            foreach (var uncommitedProjection in serializedProjections)
            {
                var category = uncommitedProjection.GetType().Name;

                var filter = FilterDefinition<BsonDocument>.Empty
                             & filterBuilder.Eq(e => e["_id"], uncommitedProjection.Id)
                             & filterBuilder.Eq(e => e["_t"], category);

                var doc = BsonDocumentWrapper.Create(uncommitedProjection);

                await projectionCollection.FindOneAndReplaceAsync(filter, doc, new FindOneAndReplaceOptions<BsonDocument>
                {
                    IsUpsert = true
                });
            }
        }
    }
}