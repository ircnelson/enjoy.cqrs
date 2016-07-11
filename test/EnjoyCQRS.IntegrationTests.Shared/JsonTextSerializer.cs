using System;
using EnjoyCQRS.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EnjoyCQRS.IntegrationTests.Shared
{
    public class JsonTextSerializer : ITextSerializer
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public string Serialize(object @object)
        {
            return JsonConvert.SerializeObject(@object, _jsonSerializerSettings);
        }

        public object Deserialize(string textSerialized, string type)
        {
            return JsonConvert.DeserializeObject(textSerialized, Type.GetType(type));
        }

        public T Deserialize<T>(string textSerialized)
        {
            return JsonConvert.DeserializeObject<T>(textSerialized, _jsonSerializerSettings);
        }
    }
}