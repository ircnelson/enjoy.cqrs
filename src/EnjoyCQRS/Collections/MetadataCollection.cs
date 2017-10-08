// The MIT License (MIT)
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

using System;
using System.Collections.Generic;
using System.Linq;

namespace EnjoyCQRS.Collections
{
    public class MetadataCollection : Dictionary<string, object>, IMetadataCollection
    {
        public static MetadataCollection Empty => new MetadataCollection();

        public MetadataCollection(IEnumerable<KeyValuePair<string, object>> keyValuePairs) : base(keyValuePairs.ToDictionary(e => e.Key, e => e.Value))
        {
        }

        private MetadataCollection()
        {   
        }

        public object GetValue(string key) => this[key];

        public T GetValue<T>(string key, Func<object, T> converter)
        {
            var value = this[key];
            return converter(value);
        }

        public IMetadataCollection Merge(IMetadataCollection metadata)
        {
            var dict = this.ToDictionary(e => e.Key, e => e.Value);
            return new MetadataCollection(metadata.Union(dict));
        }
    }
}