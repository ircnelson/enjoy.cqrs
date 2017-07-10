using System;
using EnjoyCQRS.Core;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using System.Collections.Generic;

namespace EnjoyCQRS.EventStore.MongoDB
{
    public class BsonTextSerializer : ITextSerializer
    {
        private static Dictionary<string, Type> _cache = new Dictionary<string, Type>();

        public string Serialize(object @object)
        {
            var text = BsonDocumentWrapper.Create(@object).ToJson(new JsonWriterSettings
            {
                OutputMode = JsonOutputMode.Strict
            });

            return text;
        }

        public object Deserialize(string textSerialized, string type)
        {
            var doc = BsonDocument.Parse(textSerialized);

            if (!_cache.ContainsKey(type))
            {
                _cache.Add(type, Type.GetType(type));
            }

            var obj = BsonSerializer.Deserialize(doc, _cache[type]);

            return obj;
        }

        public T Deserialize<T>(string textSerialized)
        {
            var doc = BsonDocument.Parse(textSerialized);

            var obj = BsonSerializer.Deserialize<T>(doc);

            return obj;
        }
    }
}
