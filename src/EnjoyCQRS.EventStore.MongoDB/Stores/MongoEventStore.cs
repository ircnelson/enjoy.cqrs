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
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using MongoDB.Bson.Serialization;
using EnjoyCQRS.Collections;
using EnjoyCQRS.Stores;

namespace EnjoyCQRS.EventStore.MongoDB.Stores
{
    public class MongoEventStore : IEventStore
    {
        private readonly List<IUncommittedEvent> _uncommittedEvents = new List<IUncommittedEvent>();

        private readonly IMongoDatabase _db;
        private readonly MongoEventStoreSetttings _settings;

        public JsonSerializerSettings JsonSettings { get; set; }

        public IReadOnlyList<IUncommittedEvent> Uncommitted => _uncommittedEvents.AsReadOnly();

        public MongoEventStore(IMongoDatabase db, MongoEventStoreSetttings settings)
        {
            _db = db;
            _settings = settings;
        }

        public async Task<IEnumerable<ICommittedEvent>> GetAllEventsAsync(Guid id)
        {
            var collection = _db.GetCollection<EventDocument>(_settings.EventsCollectionName);

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
            _uncommittedEvents.AddRange(collection);

            return Task.CompletedTask;
        }
        
        public EventDocument Serialize(IUncommittedEvent eventToSerialize)
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

        public ICommittedEvent Deserialize(EventDocument doc)
        {
            var eventType = Type.GetType(doc.Metadata[MetadataKeys.EventClrType].AsString);
            var data = JsonConvert.DeserializeObject(doc.EventData.ToJson(), eventType, JsonSettings);

            var metadata = BsonSerializer.Deserialize<Dictionary<string, object>>(doc.Metadata.ToJson());
            var version = doc.Metadata[MetadataKeys.EventVersion].AsInt32;

            return new MongoCommittedEvent(doc.AggregateId, version, data, new MetadataCollection(metadata));
        }

        public void Dispose()
        {
            _uncommittedEvents.Clear();
        }
    }
}