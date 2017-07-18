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

using System.Threading;
using System.Threading.Tasks;
using EnjoyCQRS.Core;
using EnjoyCQRS.Projections;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace EnjoyCQRS.EventStore.MongoDB.Projection
{
    public class MongoEventStreamReader : EventStreamReader
    {
        private readonly IMongoDatabase _database;
        private readonly ITextSerializer _textSerializer;
        private readonly MongoEventStoreSetttings _settings = new MongoEventStoreSetttings();

        public virtual int BatchSize { get; } = 400;

        public Func<FilterDefinitionBuilder<BsonDocument>, FilterDefinition<BsonDocument>> Match { get; set; }

        public MongoEventStreamReader(
            IMongoDatabase database,
            ITextSerializer textSerializer,
            MongoEventStoreSetttings settings = null)
        {
            if (settings != null)
            {
                _settings = settings;
            }

            _textSerializer = textSerializer;
            _database = database;
        }

        public override async Task ReadAsync(CancellationToken cancellationToken, OnDeserializeEventDelegate onDeserializeEvent)
        {
            var eventsCollection = _database.GetCollection<BsonDocument>(_settings.EventsCollectionName);

            var sort = new BsonDocument
            {
                {"timestamp", 1},
                {"metadata.eventVersion", 1}
            };

            var filterBuilder = new FilterDefinitionBuilder<BsonDocument>();

            var filter = FilterDefinition<BsonDocument>.Empty;

            if (Match != null)
            {
                filter = Match(filterBuilder);
            }

            var query = eventsCollection.Aggregate().Match(filter).Sort(sort);
            query.Options.AllowDiskUse = true;
            query.Options.BatchSize = BatchSize;

            var cursor = await query.ToCursorAsync().ConfigureAwait(false);

            while (await cursor.MoveNextAsync().ConfigureAwait(false))
            {
                foreach (var item in cursor.Current)
                {
                    var @event = CreateEvent(item);
                    var metadata = CreateMetadata(item);

                    onDeserializeEvent(@event, metadata);
                }
            }
        }

        private Dictionary<string, object> CreateMetadata(BsonDocument item)
        {
            return _textSerializer.Deserialize<Dictionary<string, object>>(item["metadata"].ToJson());
        }

        private object CreateEvent(BsonDocument record)
        {
            var eventId = record["_id"].AsGuid;
            var metadata = record["metadata"];
            var timestamp = record["timestamp"].ToUniversalTime();

            //var eventType = Type.GetType(metadata[EventSource.MetadataKeys.EventClrType].AsString);
            var eventType = metadata[EventSource.MetadataKeys.EventClrType].AsString;
            var eventData = record["eventData"].ToJson();

            return _textSerializer.Deserialize(eventData, eventType);
        }
    }
}
