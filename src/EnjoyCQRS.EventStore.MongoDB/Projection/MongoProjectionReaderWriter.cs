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
using System.Text;
using System.IO;
using System.Linq;
using EnjoyCQRS.Projections;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EnjoyCQRS.EventStore.MongoDB.Projection
{
    public class MongoProjectionReaderWriter<TKey, TView> : IProjectionReader<TKey, TView>, IProjectionWriter<TKey, TView>
    {
        
        private readonly IProjectionStrategy _strategy;
        private readonly IMongoCollection<BsonDocument> _collection;

        public MongoProjectionReaderWriter(IProjectionStrategy strategy, IMongoCollection<BsonDocument> collection)
        {
            _strategy = strategy;
            _collection = collection;
        }

        public TView AddOrUpdate(TKey key, Func<TView> addValueFactory, Func<TView, TView> updateValueFactory)
        {
            var result = default(TView);

            var filter = GetFilter(key);

            var query = _collection.Aggregate().Match(filter).As<TView>();

            var doc = query.SingleOrDefault();

            if (doc == null)
            {
                result = addValueFactory();
                
                using (var memory = new MemoryStream())
                {
                    _strategy.Serialize(result, memory);

                    var bytes = memory.ToArray();
                    var json = Encoding.UTF8.GetString(bytes);
                    var bsonDoc = BsonDocument.Parse(json);

                    _collection.InsertOne(bsonDoc);
                }
            }
            else
            {
                result = updateValueFactory(doc);

                using (var memory = new MemoryStream())
                {
                    _strategy.Serialize(result, memory);

                    var bytes = memory.ToArray();
                    var json = Encoding.UTF8.GetString(bytes);
                    var bsonDoc = BsonDocument.Parse(json);

                    _collection.ReplaceOne(filter, bsonDoc);
                }
            }

            return result;
        }

        public bool TryGet(TKey key, out TView view)
        {
            var filter = GetFilter(key);

            try
            {
                var doc = _collection.Find(filter).SingleOrDefault();

                if (doc != null)
                {
                    using (var stream = new MemoryStream(doc.ToBson()))
                    {
                        view = _strategy.Deserialize<TView>(stream);

                        return true;
                    }
                }
            }
            catch
            {
                // logging
            }

            view = default(TView);

            return false;
        }

        public bool TryDelete(TKey key)
        {
            var filter = GetFilter(key);

            try
            {
                _collection.DeleteOne(filter);

                return true;
            }

            catch
            {

            }

            return false;
        }

        BsonDocument GetFilter(object key)
        {
            return BsonDocument.Parse(_strategy.GetEntityLocation<TView>(key));
        }
    }
}
