﻿// The MIT License (MIT)
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

using System.IO;
using EnjoyCQRS.Core;
using EnjoyCQRS.Projections;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using System;

namespace EnjoyCQRS.EventStore.MongoDB.Projection
{
    public class MongoDocumentStrategy : IDocumentStrategy
    {
        private readonly string _viewName;

        private static ITextSerializer Serializer = new BsonTextSerializer();

        public string GetEntityBucket<TView>()
        {
            return typeof(TView).Name;
        }

        public string GetEntityLocation<TView>(object key)
        {
            var filter = new BsonDocument
            {
                { "_id", (Guid) key },
                { "_t", _viewName }
            };

            return filter.ToJson();
        }

        public void Serialize<TView>(TView entity, Stream stream)
        {
            var ser = Serializer.Serialize(entity);
            
            using (var sw = new StreamWriter(stream))
            {
                sw.WriteLine(ser);
            }
        }

        public TView Deserialize<TView>(Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                var bytes = ms.ToArray();

                return BsonSerializer.Deserialize<TView>(bytes);
            }
        }
    }
}
