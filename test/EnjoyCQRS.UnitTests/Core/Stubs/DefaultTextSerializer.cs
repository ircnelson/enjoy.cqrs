using System;
using System.Collections.Generic;
using System.Text;
using EnjoyCQRS.Core;

namespace EnjoyCQRS.UnitTests.Core.Stubs
{
    class DefaultTextSerializer : ITextSerializer
    {
        public object Deserialize(string textSerialized, string type)
        {
            return null;
        }

        public T Deserialize<T>(string textSerialized)
        {
            return (T)new object();
        }

        public string Serialize(object @object)
        {
            return null;
        }
    }
}
