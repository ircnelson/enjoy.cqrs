using System;
using EnjoyCQRS.Core;
using EnjoyCQRS.EventSource.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EnjoyCQRS.UnitTests.Shared
{
    public class JsonTextSerializer : ITextSerializer
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public string Serialize(object @object)
        {
            return JsonConvert.SerializeObject(@object, Settings);
        }

        public object Deserialize(string textSerialized, string type)
        {
            var clrType = Type.GetType(type);

            if (clrType == null) throw new EventTypeNotFoundException(type);

            return JsonConvert.DeserializeObject(textSerialized, clrType, Settings);
        }

        public T Deserialize<T>(string textSerialized)
        {
            return JsonConvert.DeserializeObject<T>(textSerialized, Settings);
        }
    }
}