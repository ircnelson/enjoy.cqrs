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

namespace EnjoyCQRS.EventSource
{
    public class Metadata : Dictionary<string, string>, IMetadata
    {
        public static Metadata Empty => new Metadata();

        public Metadata(IEnumerable<KeyValuePair<string, string>> keyValuePairs) : base(keyValuePairs.ToDictionary(e => e.Key, e => e.Value))
        {
        }

        private Metadata()
        {   
        }

        public string GetValue(string key) => this[key];

        public T GetValue<T>(string key, Func<string, T> converter)
        {
            var value = this[key];
            return converter(value);
        }
    }

    public struct MetadataKeys
    {
        public const string AggregateTypeFullname = "aggregateTypeFullname";
        public const string AggregateId = "aggregateId";
        public const string AggregateSequenceNumber = "aggregateSequenceNumber";

        public const string EventId = "eventId";
        public const string EventClrType = "eventClrType";
        public const string EventName = "eventName";
        public const string EventVersion = "eventVersion";

        public const string CorrelationId = "correlationId";
    }
}