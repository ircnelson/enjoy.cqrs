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
using System.Collections;
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

namespace EnjoyCQRS.EventStore.MongoDB
{
    public delegate Task AddOrUpdateProjectionsDelegate(IEnumerable<IProjection> projections);

    public class MongoEventStore : IEventStore
    {
        private readonly List<Event> _uncommitedEvents = new List<Event>();
        private readonly List<SnapshotData> _uncommitedSnapshots = new List<SnapshotData>();
        private readonly List<IProjection> _uncommitedProjections = new List<IProjection>();

        public MongoClient Client { get; }
        public string Database { get; }
        public MongoEventStoreSetttings Setttings { get; }

        public AddOrUpdateProjectionsDelegate AddOrUpdateProjections { get; }

        public MongoEventStore(MongoClient client, string database) : this(client, database, new MongoEventStoreSetttings())
        {
        }

        public MongoEventStore(MongoClient client, string database, MongoEventStoreSetttings setttings, AddOrUpdateProjectionsDelegate addOrUpdateProjections = null)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (database == null) throw new ArgumentNullException(nameof(database));
            if (setttings == null) throw new ArgumentNullException(nameof(setttings));

            setttings.Validate();

            Database = database;
            Setttings = setttings;
            Client = client;

            if (addOrUpdateProjections == null)
            {
                AddOrUpdateProjections = AddOrUpdateProjectionsAsync;
            }
        }

        public Task SaveSnapshotAsync(ISerializedSnapshot snapshot)
        {
            var snapshotData = Serialize(snapshot);
            _uncommitedSnapshots.Add(snapshotData);

            return Task.CompletedTask;
        }

        public async Task<ICommitedSnapshot> GetLatestSnapshotByIdAsync(Guid aggregateId)
        {
            var db = Client.GetDatabase(Database);
            var snapshotCollection = db.GetCollection<SnapshotData>(Setttings.SnapshotsCollectionName);

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
            var db = Client.GetDatabase(Database);
            var collection = db.GetCollection<Event>(Setttings.EventsCollectionName);

            var filter = Builders<Event>.Filter;
            var sort = Builders<Event>.Sort;

            var events = await collection
                .Find(filter.Eq(x => x.AggregateId, aggregateId) & filter.Gt(x => x.Version, version))
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
            if (_uncommitedEvents.Count == 0) return;

            var db = Client.GetDatabase(Database);
            var eventCollection = db.GetCollection<Event>(Setttings.EventsCollectionName);
            var snapshotCollection = db.GetCollection<SnapshotData>(Setttings.SnapshotsCollectionName);

            if (_uncommitedSnapshots.Count > 0)
                await snapshotCollection.InsertManyAsync(_uncommitedSnapshots);

            if (_uncommitedEvents.Count > 0)
                await eventCollection.InsertManyAsync(_uncommitedEvents);

            if (_uncommitedProjections.Count > 0)
            {
                await AddOrUpdateProjections(_uncommitedProjections);
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
            var collection = db.GetCollection<Event>(Setttings.EventsCollectionName);

            var filter = Builders<Event>.Filter;
            var sort = Builders<Event>.Sort;

            var events = await collection
                .Find(filter.Eq(x => x.AggregateId, id))
                .Sort(sort.Ascending(x => x.Metadata[MetadataKeys.EventVersion]))
                .ToListAsync();

            return events.Select(Deserialize).ToList();
        }

        public Task SaveAsync(IEnumerable<ISerializedEvent> collection)
        {
            var eventList = collection.Select(Serialize);
            _uncommitedEvents.AddRange(eventList);

            return Task.CompletedTask;
        }

        public Task SaveProjectionAsync(IProjection projection)
        {
            _uncommitedProjections.Add(projection);

            return Task.CompletedTask;
        }

        private Event Serialize(ISerializedEvent serializedEvent)
        {
            var eventData = BsonDocument.Parse(serializedEvent.SerializedData);
            var metadata = BsonDocument.Parse(serializedEvent.SerializedMetadata);
            var id = serializedEvent.Metadata.GetValue(MetadataKeys.EventId, value => Guid.Parse(value.ToString()));
            var eventType = serializedEvent.Metadata.GetValue(MetadataKeys.EventName, value => value.ToString());

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

        private void Cleanup()
        {
            _uncommitedEvents.Clear();
            _uncommitedSnapshots.Clear();
            _uncommitedProjections.Clear();
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
            var id = serializedSnapshot.Metadata.GetValue(MetadataKeys.SnapshotId,
                value => Guid.Parse(value.ToString()));

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

        private async Task AddOrUpdateProjectionsAsync(IEnumerable<IProjection> projections)
        {
            var db = Client.GetDatabase(Database);
            var projectionCollection = db.GetCollection<MongoProjection>(Setttings.ProjectionsCollectionName);

            var filterBuilder = new FilterDefinitionBuilder<MongoProjection>();

            foreach (var uncommitedProjection in projections.Cast<MongoProjection>())
            {
                var filter = FilterDefinition<MongoProjection>.Empty
                             & filterBuilder.Eq(e => e.Id, uncommitedProjection.Id)
                             & filterBuilder.Eq(e => e.Category, uncommitedProjection.Category);

                var document = await projectionCollection.FindAsync(filter);

                if (await document.AnyAsync())
                {
                    var update = Builders<MongoProjection>.Update;

                    var updateDefinition = update
                        .Set(e => e.Projection, uncommitedProjection.Projection);

                    await projectionCollection.FindOneAndUpdateAsync(filter, updateDefinition);
                }
                else
                {
                    await projectionCollection.InsertOneAsync(uncommitedProjection);
                }
            }
        }
    }
}