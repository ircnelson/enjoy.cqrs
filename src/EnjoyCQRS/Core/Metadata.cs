using System.Collections.Generic;

namespace EnjoyCQRS.Core
{
    public class Metadata<T>
    {
        private readonly Dictionary<string, object> _metadata;
        public T Data { get; }

        public Metadata(T data, Dictionary<string, object> metadata)
        {
            Data = data;
            _metadata = metadata;
        }

        public bool ContainsKey(string key)
        {
            return _metadata.ContainsKey(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return _metadata.TryGetValue(key, out value);
        }

        public object GetValue(string key)
        {
            if (_metadata.ContainsKey(key))
            {
                return _metadata[key];
            }

            return null;
        }
    }
}
