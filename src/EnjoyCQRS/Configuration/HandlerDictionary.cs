using System;
using System.Collections.Generic;

namespace EnjoyCQRS.Configuration
{
    public class HandlerDictionary : Dictionary<HandlerMetadata, IList<Type>>
    {
        public HandlerDictionary()
        {
        }

        public HandlerDictionary(IDictionary<HandlerMetadata, IList<Type>> collection)
        {
            foreach (var item in collection)
            {
                Add(item.Key, item.Value);
            }
        }
    }
}