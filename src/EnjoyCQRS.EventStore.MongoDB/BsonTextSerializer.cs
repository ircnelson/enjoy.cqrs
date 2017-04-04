using System;
using EnjoyCQRS.Core;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace EnjoyCQRS.EventStore.MongoDB
{
    public class BsonTextSerializer : ITextSerializer
    {
        public string Serialize(object @object)
        {
            var ser = BsonDocumentWrapper.Create(@object).ToJson(new JsonWriterSettings
            {
                OutputMode = JsonOutputMode.Strict
            });

            return ser;
        }

        public object Deserialize(string textSerialized, string type)
        {
            var doc = BsonDocument.Parse(textSerialized);

            var obj = BsonSerializer.Deserialize(doc, Type.GetType(type));

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
