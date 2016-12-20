using System;
using EnjoyCQRS.Core;
using EnjoyCQRS.EventSource.Exceptions;
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
            var clrType = Type.GetType(type);

            if (clrType == null) throw new EventTypeNotFoundException(type);

            return JsonConvert.DeserializeObject(textSerialized, clrType);
        }

        public T Deserialize<T>(string textSerialized)
        {
            return JsonConvert.DeserializeObject<T>(textSerialized);
        }
    }
}