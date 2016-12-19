using System;
using EnjoyCQRS.Core;
using Newtonsoft.Json;

namespace EnjoyCQRS.UnitTests.Shared
{
    public class JsonTextSerializer : ITextSerializer
    {
        public string Serialize(object @object)
        {
            return JsonConvert.SerializeObject(@object);
        }

        public object Deserialize(string textSerialized, string type)
        {
            return JsonConvert.DeserializeObject(textSerialized, Type.GetType(type));
        }

        public T Deserialize<T>(string textSerialized)
        {
            return JsonConvert.DeserializeObject<T>(textSerialized);
        }
    }
}