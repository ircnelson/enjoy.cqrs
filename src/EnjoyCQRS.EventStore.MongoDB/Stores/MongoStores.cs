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
using EnjoyCQRS.EventSource.Projections;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using EnjoyCQRS.Projections;
using EnjoyCQRS.EventStore.MongoDB.Projection;
using EnjoyCQRS.Core;
using EnjoyCQRS.Stores;
using EnjoyCQRS.EventSource.Storage;
using IProjectionStoreV1 = EnjoyCQRS.EventSource.Projections.IProjectionStore;

namespace EnjoyCQRS.EventStore.MongoDB.Stores
{
    public class MongoStores : ITransaction, ICompositeStores
    {
        private readonly IProjectionRebuilder _projectionRebuilder;
        private readonly MongoEventStreamReader _eventStreamReader;
        private readonly MongoEventStore _eventStore;
        private readonly MongoSnapshotStore _snapshotStore;
        private readonly ProjectionStoreV1 _projectionStoreV1;
                
        public JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public IMongoClient Client { get; }
        public string DatabaseName { get; }
        public MongoEventStoreSetttings Settings { get; }

        public IEventStore EventStore => _eventStore;

        public ISnapshotStore SnapshotStore => _snapshotStore;

        public IProjectionStoreV1 ProjectionStoreV1 => _projectionStoreV1;

        public MongoStores(IMongoDatabase database) : this(database, null, null, new MongoEventStoreSetttings())
        {
        }

        public MongoStores(IMongoDatabase database,
                           IProjectionRebuilder projectionRebuilder = null,
                           MongoEventStreamReader eventStreamReader = null,
                           MongoEventStoreSetttings setttings = null)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));

            _projectionRebuilder = projectionRebuilder;
            _eventStreamReader = eventStreamReader;

            if (setttings == null)
            {
                setttings = new MongoEventStoreSetttings();
            }

            setttings.Validate();

            Settings = setttings;
            Client = database.Client;
            DatabaseName = database.DatabaseNamespace.DatabaseName;

            _eventStore = new MongoEventStore(database, setttings)
            {
                JsonSettings = JsonSettings
            };

            _snapshotStore = new MongoSnapshotStore(_eventStore, database, setttings);
            _projectionStoreV1 = new ProjectionStoreV1();
        }

        public MongoStores(MongoClient client, string database) : this(client, database, new MongoEventStoreSetttings())
        {
        }

        public MongoStores(MongoClient client, string database, MongoEventStoreSetttings setttings = null)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
            DatabaseName = database ?? throw new ArgumentNullException(nameof(database));

            if (setttings == null) throw new ArgumentNullException(nameof(setttings));

            setttings.Validate();

            Settings = setttings;

            var db = Client.GetDatabase(database);

            _eventStore = new MongoEventStore(db, setttings)
            {
                JsonSettings = JsonSettings
            };

            _snapshotStore = new MongoSnapshotStore(_eventStore, db, setttings);
            _projectionStoreV1 = new ProjectionStoreV1();
        }

        public void BeginTransaction()
        {
        }

        public async Task CommitAsync()
        {
            var db = Client.GetDatabase(DatabaseName);

            var uncommittedSnapshots = _snapshotStore.Uncommitted;

            if (uncommittedSnapshots.Count > 0)
            {
                var snapshotCollection = db.GetCollection<SnapshotDocument>(Settings.SnapshotsCollectionName);

                var serializedSnapshots = uncommittedSnapshots.Select(_snapshotStore.Serialize);

                await snapshotCollection.InsertManyAsync(serializedSnapshots);
            }

            var uncommittedEvents = _eventStore.Uncommitted;

            if (uncommittedEvents.Count > 0)
            {
                var eventCollection = db.GetCollection<EventDocument>(Settings.EventsCollectionName);

                var serializedEvents = uncommittedEvents.Select(_eventStore.Serialize);

                await eventCollection.InsertManyAsync(serializedEvents);

                var eventIds = serializedEvents.Select(e => e.Id).ToList();

                if (_eventStreamReader != null)
                {
                    _eventStreamReader.Match = builder => builder.In("_id", eventIds.ToArray());

                    await _projectionRebuilder.RebuildAsync(_eventStreamReader).ConfigureAwait(false);
                }
            }

            var uncommittedProjections = _projectionStoreV1.Uncommitted;

            if (uncommittedProjections.Count > 0)
            {
                await AddOrUpdateProjectionsAsync(uncommittedProjections);
            }

            Cleanup();
        }

        public void Rollback()
        {
            Cleanup();
        }
        
        private void Cleanup()
        {
            //UncommittedEvents.Clear();
            //UncommittedSnapshots.Clear();
            //UncommittedProjections.Clear();
        }

        protected virtual async Task AddOrUpdateProjectionsAsync(IEnumerable<IProjection> uncommittedProjections)
        {
            var db = Client.GetDatabase(DatabaseName);
            var collection = db.GetCollection<BsonDocument>(Settings.ProjectionsCollectionName);

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

        public void Dispose()
        {
        }
    }
}