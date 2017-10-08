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
using EnjoyCQRS.EventSource.Snapshots;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using EnjoyCQRS.Collections;
using EnjoyCQRS.Stores;

namespace EnjoyCQRS.EventStore.MongoDB.Stores
{
    public class MongoSnapshotStore : ISnapshotStore
    {
        private readonly List<IUncommittedSnapshot> _uncommittedSnapshots = new List<IUncommittedSnapshot>();

        private readonly MongoEventStore _eventStore;
        private readonly IMongoDatabase _db;
        private readonly MongoEventStoreSetttings _settings;

        public IReadOnlyList<IUncommittedSnapshot> Uncommitted => _uncommittedSnapshots.AsReadOnly();

        public MongoSnapshotStore(MongoEventStore eventStore, IMongoDatabase db, MongoEventStoreSetttings settings)
        {
            _eventStore = eventStore;
            _db = db;
            _settings = settings;
        }

        public Task SaveSnapshotAsync(IUncommittedSnapshot snapshot)
        {
            _uncommittedSnapshots.Add(snapshot);

            return Task.CompletedTask;
        }

        public async Task<ICommittedSnapshot> GetLatestSnapshotByIdAsync(Guid aggregateId)
        {
            var snapshotCollection = _db.GetCollection<SnapshotDocument>(_settings.SnapshotsCollectionName);

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
            return _eventStore.GetEventsForwardAsync(aggregateId, version);
        }

        public SnapshotDocument Serialize(IUncommittedSnapshot serializedSnapshot)
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

        public ICommittedSnapshot Deserialize(SnapshotDocument doc)
        {
            var snapshotType = Type.GetType(doc.Metadata[MetadataKeys.SnapshotClrType].AsString);

            var data = (ISnapshot)BsonSerializer.Deserialize(doc.Data, snapshotType);
            var metadata = BsonSerializer.Deserialize<Dictionary<string, object>>(doc.Metadata.ToJson());

            return new MongoCommittedSnapshot(doc.AggregateId, doc.Version, data, new MetadataCollection(metadata));
        }
        
        public void Dispose()
        {
            _uncommittedSnapshots.Clear();
        }
    }
}